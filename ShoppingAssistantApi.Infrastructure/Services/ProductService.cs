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

    public ProductService(IOpenAiService openAiService, IWishlistsService wishlistsService)
    {
        _openAiService = openAiService;
        _wishlistsService = wishlistsService;
    }
    
    public async IAsyncEnumerable<ServerSentEvent> SearchProductAsync(string wishlistId, MessageCreateDto message, CancellationToken cancellationToken)
    {
        var chatRequest = new ChatCompletionRequest
        {
            Messages = new List<OpenAiMessage>
            {
                new OpenAiMessage
                {
                    Role = "User",
                    Content = ""
                }
            },
            Stream = true
        };

        var suggestionBuffer = new Suggestion();
        var messageBuffer = new MessagePart();
        var currentDataType = SearchEventType.Wishlist;
        var dataTypeHolder = string.Empty;
        var dataBuffer = string.Empty;

        await foreach (var data in _openAiService.GetChatCompletionStream(chatRequest, cancellationToken))
        {
            if (data.Contains("["))
            {
                if (dataTypeHolder=="[Message]" && messageBuffer.Text!=null)
                {
                    _wishlistsService.AddMessageToPersonalWishlistAsync(wishlistId, new MessageCreateDto()
                    {
                        Text = messageBuffer.Text,
                    }, cancellationToken);
                }
                dataTypeHolder = string.Empty;
                dataTypeHolder += data;
            }

            else if (data.Contains("]"))
            {
                dataTypeHolder += data;
                currentDataType = DetermineDataType(dataTypeHolder);
            }

            else if (dataTypeHolder=="[" && !data.Contains("["))
            {
                dataTypeHolder += data;
            }

            else
            {
                dataBuffer += data;
                
                switch (currentDataType)
                {
                    case SearchEventType.Message:
                        yield return new ServerSentEvent
                        {
                            Event = SearchEventType.Message,
                            Data = data
                        };
                        messageBuffer.Text += data;
                        break;

                    case SearchEventType.Suggestion:
                        suggestionBuffer.Text += data;
                        if (data.Contains(";"))
                        {
                            yield return new ServerSentEvent
                            {
                                Event = SearchEventType.Suggestion,
                                Data = suggestionBuffer.Text
                            };
                            suggestionBuffer.Text = string.Empty;
                        } 
                        break;
                    case SearchEventType.Product:
                        yield return new ServerSentEvent
                        {
                            Event = SearchEventType.Product,
                            Data = data
                        };
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