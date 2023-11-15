using System.Diagnostics;
using MongoDB.Bson;
using ShoppingAssistantApi.Application.IRepositories;
using ShoppingAssistantApi.Application.IServices;
using ShoppingAssistantApi.Application.Models.CreateDtos;
using ShoppingAssistantApi.Application.Models.Dtos;
using ShoppingAssistantApi.Application.Models.OpenAi;
using ShoppingAssistantApi.Application.Models.ProductSearch;
using ShoppingAssistantApi.Domain.Entities;
using ShoppingAssistantApi.Domain.Enums;
using ServerSentEvent = ShoppingAssistantApi.Application.Models.ProductSearch.ServerSentEvent;

namespace ShoppingAssistantApi.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly IWishlistsService _wishlistsService;
    
    private readonly IOpenAiService _openAiService;

    private readonly IMessagesRepository _messagesRepository;
    
    private bool mqchecker = false;
    
    private SearchEventType currentDataType = SearchEventType.Wishlist;

    public ProductService(IOpenAiService openAiService, IWishlistsService wishlistsService, IMessagesRepository messagesRepository)
    {
        _openAiService = openAiService;
        _wishlistsService = wishlistsService;
        _messagesRepository = messagesRepository;
    }
    
    public async IAsyncEnumerable<ServerSentEvent> SearchProductAsync(string wishlistId, MessageCreateDto message, CancellationToken cancellationToken)
    {
        string promptForGpt =
            "You are a Shopping Assistant that helps people find product recommendations. Ask user additional questions if more context needed." +
            "\nYou must return data with one of the prefixes:" +
            "\n[Question] - return question" +
            "\n[Suggestions] - return semicolon separated suggestion how to answer to a question" +
            "\n[Message] - return text" +
            "\n[Products] - return semicolon separated product names";
        
        var countOfMessage = await _messagesRepository
            .GetCountAsync(message=>message.WishlistId==ObjectId.Parse((wishlistId)), cancellationToken);

        var previousMessages = await _wishlistsService
            .GetMessagesPageFromPersonalWishlistAsync(wishlistId, 1, countOfMessage, cancellationToken);

        var chatRequest = new ChatCompletionRequest
        {
            Messages = new List<OpenAiMessage>
            {
                new OpenAiMessage
                {
                    Role = OpenAiRoleExtensions.RequestConvert(OpenAiRole.System),
                    Content = promptForGpt
                }
            }
        };
        
        
        var messagesForOpenAI = new List<OpenAiMessage>();
        
        foreach (var item in previousMessages.Items)
        {
            messagesForOpenAI
                .Add(new OpenAiMessage() 
                {
                    Role = item.Role.ToLower(),
                    Content = item.Text 
                });
        }
            
        messagesForOpenAI.Add(new OpenAiMessage()
        {
            Role = OpenAiRoleExtensions.RequestConvert(OpenAiRole.User),
            Content = message.Text
        });
            
        chatRequest.Messages.AddRange(messagesForOpenAI);
            
        var suggestionBuffer = new Suggestion();
        var messageBuffer = new MessagePart();
        var productBuffer = new ProductName();
        var dataTypeHolder = string.Empty;
        var counter = 0;

        await foreach (var data in _openAiService.GetChatCompletionStream(chatRequest, cancellationToken))
        {
            counter++;
            if (mqchecker && currentDataType == SearchEventType.Message && messageBuffer != null)
            {
                if (data == "[")
                {
                    _wishlistsService.AddMessageToPersonalWishlistAsync(wishlistId, new MessageDto()
                    {
                        Text = messageBuffer.Text,
                        Role = MessageRoles.Application.ToString(),
                    }, cancellationToken);
                    mqchecker = false;
                }
            }
            
            if (data.Contains("["))
            {
                dataTypeHolder = string.Empty;
                dataTypeHolder += data;
            }

            else if (data.Contains("]"))
            {
                dataTypeHolder += data;
                currentDataType = DetermineDataType(dataTypeHolder);
                if (currentDataType == SearchEventType.Message)
                {
                    mqchecker = true;
                }
            }

            else if (dataTypeHolder=="[" && !data.Contains("["))
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
                        if (data.Contains(";"))
                        {
                            yield return new ServerSentEvent
                            {
                                Event = SearchEventType.Suggestion,
                                Data = suggestionBuffer.Text
                            };
                            suggestionBuffer.Text = string.Empty;
                            break;
                        } 
                        suggestionBuffer.Text += data;
                        break;
                        
                    case SearchEventType.Product:
                        if (data.Contains(";"))
                        {
                            yield return new ServerSentEvent
                            {
                                Event = SearchEventType.Product,
                                Data = productBuffer.Name
                            };
                            productBuffer.Name = string.Empty;
                                
                            //a complete description of the entity when the Amazon API is connected
                            await _wishlistsService.AddProductToPersonalWishlistAsync(wishlistId, new ProductCreateDto()
                            {
                                Url = "",
                                Name = productBuffer.Name,
                                Rating = 0,
                                Description = "",
                                Price = 0,
                                ImagesUrls = new []{"", ""},
                                WasOpened = false
                            }, cancellationToken);
                            break;
                        }
                        productBuffer.Name += data;
                        break;  
                }
            }
        }
        if (currentDataType == SearchEventType.Message)
        {
            _wishlistsService.AddMessageToPersonalWishlistAsync(wishlistId, new MessageDto()
            {
                Text = messageBuffer.Text,
                Role = MessageRoles.Application.ToString(),
            }, cancellationToken);
            mqchecker = false;
        }
    }

    private SearchEventType DetermineDataType(string dataTypeHolder)
    {
        if (dataTypeHolder.StartsWith("[Question]"))
        {
            return SearchEventType.Message;
        }
        else if (dataTypeHolder.StartsWith("[Options]"))
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