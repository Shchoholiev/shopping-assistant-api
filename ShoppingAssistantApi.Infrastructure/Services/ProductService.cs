using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Newtonsoft.Json.Linq;
using ShoppingAssistantApi.Application.IRepositories;
using ShoppingAssistantApi.Application.IServices;
using ShoppingAssistantApi.Application.Models.CreateDtos;
using ShoppingAssistantApi.Application.Models.OpenAi;
using ShoppingAssistantApi.Application.Models.ProductSearch;
using ShoppingAssistantApi.Domain.Entities;
using ShoppingAssistantApi.Domain.Enums;

namespace ShoppingAssistantApi.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly IWishlistsRepository _wishlistsRepository;
    
    private readonly IWishlistsService _wishlistsService;
    
    private readonly IOpenAiService _openAiService;

    public ProductService(IOpenAiService openAiService, IWishlistsService wishlistsService, IWishlistsRepository wishlistsRepository)
    {
        _openAiService = openAiService;
        _wishlistsService = wishlistsService;
        _wishlistsRepository = wishlistsRepository;
    }

    public async Task<List<string>> StartNewSearchAndReturnWishlist(Message message,
        CancellationToken cancellationToken)
    {
        List<OpenAiMessage> messages = new List<OpenAiMessage>()
        {
            new OpenAiMessage()
            {
                Role = OpenAiRole.User,
                Content = PromptForProductSearch(message.Text)
            }
        };
        
        var chatRequest = new ChatCompletionRequest
        {
            Messages = messages
        };

        var openAiMessage = await _openAiService.GetChatCompletion(chatRequest, cancellationToken);

        var openAiContent = JObject.Parse(openAiMessage.Content);
        var productNames = openAiContent["Name"]?.ToObject<List<ProductName>>() ?? new List<ProductName>();


        WishlistCreateDto newWishlist = new WishlistCreateDto()
        {
            Type = "Product",
            FirstMessageText = message.Text
        };

        var resultWishList =  _wishlistsService.StartPersonalWishlistAsync(newWishlist, cancellationToken);
        
        return productNames.Select(productName => productName.Name).ToList();
    }

    public async Task<List<string>> GetProductFromSearch(Message message, CancellationToken cancellationToken)
    {
        List<OpenAiMessage> messages = new List<OpenAiMessage>()
        {
            new OpenAiMessage()
            {
                Role = OpenAiRole.User,
                Content = PromptForProductSearch(message.Text)
            }
        };
        
        var chatRequest = new ChatCompletionRequest
        {
            Messages = messages
        };

        var openAiMessage = await _openAiService.GetChatCompletion(chatRequest, cancellationToken);

        var openAiContent = JObject.Parse(openAiMessage.Content);
        var productNames = openAiContent["Name"]?.ToObject<List<ProductName>>() ?? new List<ProductName>();
        
        return productNames.Select(productName => productName.Name).ToList();
    }

    public async Task<List<string>> GetRecommendationsForProductFromSearch(Message message, CancellationToken cancellationToken)
    {
        List<OpenAiMessage> messages = new List<OpenAiMessage>()
        {
            new OpenAiMessage()
            {
                Role = OpenAiRole.User,
                Content = PromptForRecommendationsForProductSearch(message.Text)
            }
        };
    
        var chatRequest = new ChatCompletionRequest
        {
            Messages = messages
        };

        var openAiMessage = await _openAiService.GetChatCompletion(chatRequest, cancellationToken);

        var openAiContent = JObject.Parse(openAiMessage.Content);
        var recommendations = openAiContent["Recommendation"]?.ToObject<List<string>>() ?? new List<string>();
    
        return recommendations;
    }
    
    public string PromptForProductSearch(string message)
    {
        string promptForSearch = "Return information in JSON. " +
                                 "\nProvide information, only that indicated in the type of answer, namely only the name. " +
                                 "\nAsk additional questions to the user if there is not enough information. " +
                                 "\nIf there are several answer options, list them. " +
                                 "\nYou don't need to display questions and products together! " +
                                 "\nDo not output any text other than JSON!!! " +
                                 $"\n\nQuestion: {message} " +
                                 $"\nType of answer: Question:<question>[] " +
                                 $"\n\nif there are no questions, then just display the products " +
                                 $"\nType of answer: Name:<name>";
        return  promptForSearch;
    }
    
    public string PromptForRecommendationsForProductSearch(string message)
    {
        string promptForSearch = "Return information in JSON. " +
                                 "\nProvide only information indicated in the type of answer, namely only the recommendation. " +
                                 "\nIf there are several answer options, list them. " +
                                 "\nDo not output any text other than JSON." +
                                 $"\n\nGive recommendations for this question: {message} " +
                                 "\nType of answer: " +
                                 "\n\nRecommendation :<Recommendation>";
        return  promptForSearch;
    }
}