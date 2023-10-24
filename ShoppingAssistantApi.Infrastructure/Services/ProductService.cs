using System.Diagnostics;
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
        var isFirstMessage = _wishlistsService
            .GetMessagesPageFromPersonalWishlistAsync(wishlistId, 1, 1, cancellationToken).Result;

        var chatRequest = new ChatCompletionRequest();
        
        if (isFirstMessage==null)
        {
            chatRequest = new ChatCompletionRequest
            {
                Messages = new List<OpenAiMessage>
                {
                    new OpenAiMessage
                    {
                        Role = OpenAiRole.System.ToString(),
                        Content = "You are a Shopping Assistant that helps people find product recommendations. Ask user additional questions if more context needed." +
                                  "\nYou must return data with one of the prefixes:" +
                                  "\n[Question] - return question" +
                                  "\n[Suggestions] - return semicolon separated suggestion how to answer to a question" +
                                  "\n[Message] - return text" +
                                  "\n[Products] - return semicolon separated product names"
                    },
                
                    new OpenAiMessage()
                    {
                        Role = OpenAiRole.Assistant.ToString(),
                        Content = "What are you looking for?"
                    }
                },
                Stream = true
            };
            
            _wishlistsService.AddMessageToPersonalWishlistAsync(wishlistId, new MessageCreateDto()
            {
                Text = "What are you looking for?",
            }, cancellationToken);
            
            yield return new ServerSentEvent
            {
                Event = SearchEventType.Message,
                Data = "What are you looking for?"
            };
            
            yield return new ServerSentEvent
            {
                Event = SearchEventType.Suggestion,
                Data = "Bicycle"
            };
            
            yield return new ServerSentEvent
            {
                Event = SearchEventType.Suggestion,
                Data = "Laptop"
            };
        }

        if(isFirstMessage!=null)
        {
            var previousMessages = _wishlistsService
                .GetMessagesPageFromPersonalWishlistAsync(wishlistId, 1, 1, cancellationToken).Result.Items.ToList();

            var messagesForOpenAI = new List<OpenAiMessage>();
            foreach (var item in previousMessages )
            {
                messagesForOpenAI.Add(
                    new OpenAiMessage()
                {
                    Role = item.Role,
                    Content = item.Text
                });
            }
            
            chatRequest = new ChatCompletionRequest
            {
                Messages = messagesForOpenAI,
                Stream = true
            };
            
            var suggestionBuffer = new Suggestion();
            var messageBuffer = new MessagePart();
            var productBuffer = new ProductName();
            var currentDataType = SearchEventType.Wishlist;
            var dataTypeHolder = string.Empty;

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
                    if (dataTypeHolder=="[Products]" && productBuffer.Name!=null)
                    {
                        _wishlistsService.AddProductToPersonalWishlistAsync(wishlistId, new ProductCreateDto()
                        {
                            Url = "",
                            Name = productBuffer.Name,
                            Rating = 0,
                            Description = "",
                            ImagesUrls = new []{"", ""},
                            WasOpened = false
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
                            productBuffer.Name += data;
                            if (data.Contains(";"))
                            {
                                yield return new ServerSentEvent
                                {
                                    Event = SearchEventType.Product,
                                    Data = productBuffer.Name
                                };
                                productBuffer.Name = string.Empty;
                            }
                            break;  
                    }
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