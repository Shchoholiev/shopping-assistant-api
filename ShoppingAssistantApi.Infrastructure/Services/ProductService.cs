using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using ShoppingAssistantApi.Application.IRepositories;
using ShoppingAssistantApi.Application.IServices;
using ShoppingAssistantApi.Application.Models.CreateDtos;
using ShoppingAssistantApi.Application.Models.Dtos;
using ShoppingAssistantApi.Application.Models.OpenAi;
using ShoppingAssistantApi.Application.Models.ProductSearch;
using ShoppingAssistantApi.Domain.Enums;
using ServerSentEvent = ShoppingAssistantApi.Application.Models.ProductSearch.ServerSentEvent;

namespace ShoppingAssistantApi.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly IWishlistsService _wishlistsService;

    private readonly IOpenAiService _openAiService;

    private readonly IMessagesRepository _messagesRepository;

    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IOpenAiService openAiService, 
        IWishlistsService wishlistsService, 
        IMessagesRepository messagesRepository,
        ILogger<ProductService> logger)
    {
        _openAiService = openAiService;
        _wishlistsService = wishlistsService;
        _messagesRepository = messagesRepository;
        _logger = logger;
    }
    
    public async IAsyncEnumerable<ServerSentEvent> SearchProductAsync(string wishlistId, MessageCreateDto newMessage, CancellationToken cancellationToken)
    {
        var systemPrompt =
            "You are a Shopping Assistant that helps people find product recommendations. Ask user additional questions if more context needed." +
            "\nYou must return data with one of the prefixes:" +
            "\n[Question] - return question. Must be followed by suggestions how to answer the question" +
            "\n[Suggestions] - return semicolon separated suggestion how to answer to a question" +
            "\n[Message] - return text" +
            "\n[Products] - return semicolon separated product names";
        
        var wishlistObjectId = ObjectId.Parse(wishlistId);
        var messages = await _messagesRepository.GetWishlistMessagesAsync(wishlistObjectId, cancellationToken);

        var chatRequest = new ChatCompletionRequest
        {
            Messages = new List<OpenAiMessage>
            {
                new() {
                    Role = OpenAiRole.System.ToRequestString(),
                    Content = systemPrompt
                }
            }
        };
        
        for (int i = 0; i < messages.Count; i++)
        {
            var message = messages[i];
            if (i == 0)
            {
                message.Text = "[Question] " + message.Text + "\n [Suggestions] Bicycle, Laptop";
            }

            chatRequest.Messages
                .Add(new OpenAiMessage()
                {
                    Role = message.Role == "Application" ? "assistant" : "user",
                    Content = message.Text
                });
        }
            
        chatRequest.Messages.Add(new ()
        {
            Role = OpenAiRole.User.ToRequestString(),
            Content = newMessage.Text
        });

        // Don't wait for the task to finish because we dont need the result of this task
        var dto = new MessageDto()
        {
            Text = newMessage.Text,
            Role = MessageRoles.User.ToString(),
        };
        var saveNewMessageTask = _wishlistsService.AddMessageToPersonalWishlistAsync(wishlistId, dto, cancellationToken);
        
        var currentDataType = SearchEventType.Wishlist;
        var suggestionBuffer = new Suggestion();
        var messageBuffer = new MessagePart();
        var productBuffer = new ProductName();
        var dataTypeHolder = string.Empty;

        await foreach (var data in _openAiService.GetChatCompletionStream(chatRequest, cancellationToken))
        {
            if (data.Contains('['))
            {
                dataTypeHolder = data;
            }
            else if (data.Contains(']'))
            {
                if (currentDataType == SearchEventType.Message)
                {
                    _ = await saveNewMessageTask;
                    // Don't wait for the task to finish because we dont need the result of this task
                    _ = _wishlistsService.AddMessageToPersonalWishlistAsync(wishlistId, new MessageDto()
                    {
                        Text = messageBuffer.Text,
                        Role = MessageRoles.Application.ToString(),
                    }, cancellationToken);
                }

                dataTypeHolder += data;
                currentDataType = DetermineDataType(dataTypeHolder);

                dataTypeHolder = string.Empty;
            }
            else if (dataTypeHolder.Contains('['))
            {
                dataTypeHolder += data;
            }
            else
            {
                switch (currentDataType)
                {
                    case SearchEventType.Message:
                        yield return new ServerSentEvent
                        {
                            Event = SearchEventType.Message,
                            Data = data
                        };
                        currentDataType = SearchEventType.Message;
                        messageBuffer.Text += data;

                        break;

                    case SearchEventType.Suggestion:
                        if (data.Contains(';'))
                        {
                            yield return new ServerSentEvent
                            {
                                Event = SearchEventType.Suggestion,
                                Data = suggestionBuffer.Text.Trim()
                            };
                            suggestionBuffer.Text = string.Empty;
                            break;
                        } 

                        suggestionBuffer.Text += data;

                        break;
                        
                    case SearchEventType.Product:
                        if (data.Contains(';'))
                        {
                            yield return new ServerSentEvent
                            {
                                Event = SearchEventType.Product,
                                Data = productBuffer.Name.Trim()
                            };
                            productBuffer.Name = string.Empty;

                            break;
                        }

                        productBuffer.Name += data;

                        break;  
                }
            }
        }
    }

    private SearchEventType DetermineDataType(string dataTypeHolder)
    {
        if (dataTypeHolder.StartsWith("[Question]"))
        {
            return SearchEventType.Message;
        }
        else if (dataTypeHolder.StartsWith("[Suggestions]"))
        {
            return SearchEventType.Suggestion;
        }
        else if (dataTypeHolder.StartsWith("[Message]"))
        {
            return SearchEventType.Message;
        }
        else if (dataTypeHolder.StartsWith("[Products]"))
        {
            return SearchEventType.Product;
        }
        else
        {
            return SearchEventType.Wishlist;
        }
    }
}