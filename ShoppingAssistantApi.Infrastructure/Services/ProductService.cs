﻿using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;
using ShoppingAssistantApi.Application.IRepositories;
using ShoppingAssistantApi.Application.IServices;
using ShoppingAssistantApi.Application.Models.CreateDtos;
using ShoppingAssistantApi.Application.Models.Dtos;
using ShoppingAssistantApi.Application.Models.OpenAi;
using ShoppingAssistantApi.Application.Models.ProductSearch;
using ShoppingAssistantApi.Domain.Entities;
using ShoppingAssistantApi.Domain.Enums;

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

    public IAsyncEnumerable<ServerSentEvent> SearchProductAsync(string wishlistId, MessageCreateDto message, CancellationToken cancellationToken)
    {
        // get all messages from wishlist

        throw new NotImplementedException();
    }

    public async IAsyncEnumerable<(List<ProductName> ProductNames, WishlistDto Wishlist)> StartNewSearchAndReturnWishlist(Message message, CancellationToken cancellationToken)
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

        await foreach (var response in _openAiService.GetChatCompletionStream(chatRequest, cancellationToken))
        {
            var openAiContent = JObject.Parse(response);
            var productNames = openAiContent["Name"]?.ToObject<List<ProductName>>() ?? new List<ProductName>();

            WishlistCreateDto newWishlist = new WishlistCreateDto()
            {
                Type = "Product",
                FirstMessageText = message.Text
            };

            var resultWishlistTask = _wishlistsService.StartPersonalWishlistAsync(newWishlist, cancellationToken);
            var resultWishlist = await resultWishlistTask;

            yield return (productNames, resultWishlist);
        }
    }

    public async IAsyncEnumerable<string> GetProductFromSearch(Message message, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        List<OpenAiMessage> messages = new List<OpenAiMessage>()
        {
            new OpenAiMessage()
            {
                Role = OpenAiRole.User,
                Content = PromptForProductSearchWithQuestion(message.Text)
            }
        };

        var chatRequest = new ChatCompletionRequest
        {
            Messages = messages
        };

        await foreach (var response in _openAiService.GetChatCompletionStream(chatRequest, cancellationToken))
        {
            var openAiContent = JObject.Parse(response);
            var productNames = openAiContent["Name"]?.ToObject<List<ProductName>>();

            if (productNames != null && productNames.Any())
            {
                foreach (var productName in productNames)
                {
                    yield return productName.Name;
                }
            }
            else
            {
                var questions = openAiContent["AdditionalQuestion"]?.ToObject<List<Question>>() ?? new List<Question>();
            
                foreach (var question in questions)
                {
                    yield return question.QuestionText;
                }
            }
        }
    }


    public async IAsyncEnumerable<string> GetRecommendationsForProductFromSearchStream(Message message, CancellationToken cancellationToken)
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

        await foreach (var response in _openAiService.GetChatCompletionStream(chatRequest, cancellationToken))
        {
            var openAiContent = JObject.Parse(response);
            var recommendations = openAiContent["Recommendation"]?.ToObject<List<string>>() ?? new List<string>();

            foreach (var recommendation in recommendations)
            {
                yield return recommendation;
            }
        }
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
    
    public string PromptForProductSearchWithQuestion(string message)
    {
        string promptForSearch = "Return information in JSON. " +
                                 "\nAsk additional questions to the user if there is not enough information." +
                                 "\nIf there are several answer options, list them. " +
                                 "\nYou don't need to display questions and products together!" +
                                 "\nDo not output any text other than JSON!!!" +
                                 $"\n\nQuestion: {message}" +
                                 "\n\nif you can ask questions to clarify the choice, then ask them" +
                                 "\nType of answer:" +
                                 "\nAdditionalQuestion:<question>[]" +
                                 "\n\nif there are no questions, then just display the products" +
                                 "\nType of answer:" +
                                 "\nName:<name>";
        return  promptForSearch;
    }
}