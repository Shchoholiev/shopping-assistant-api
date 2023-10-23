using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using Moq;
using Newtonsoft.Json.Linq;
using ShoppingAssistantApi.Application.IServices;
using ShoppingAssistantApi.Application.Models.CreateDtos;
using ShoppingAssistantApi.Application.Models.Dtos;
using ShoppingAssistantApi.Application.Models.OpenAi;
using ShoppingAssistantApi.Application.Models.ProductSearch;
using ShoppingAssistantApi.Domain.Entities;
using ShoppingAssistantApi.Domain.Enums;
using ShoppingAssistantApi.Infrastructure.Services;

namespace ShoppingAssistantApi.Tests.Tests;

public class ProductTests
{
    private Mock<IOpenAiService> _openAiServiceMock;

    private IProductService _productService;

    public Mock<IWishlistsService> _wishListServiceMock;

    public ProductTests()
    {
        _openAiServiceMock = new Mock<IOpenAiService>();
        _wishListServiceMock = new Mock<IWishlistsService>();
        _productService = new ProductService(_openAiServiceMock.Object, _wishListServiceMock.Object);
    }
    
    
    
    /*[Fact]
    public async Task SearchProductAsync_WhenWishlistIdIsEmpty_CreatesWishlistAndReturnsEvent()
    {
        // Arrange
        string wishlistId = string.Empty; // Simulating an empty wishlist ID
        var message = new MessageCreateDto
        {
            Text = "Your message text here"
        };
        var cancellationToken = CancellationToken.None;

        // Define your expected new wishlist and event data
        var newWishlistId = "123"; // Example wishlist ID
        var expectedEvent = new ServerSentEvent
        {
            Event = SearchEventType.Wishlist,
            Data = newWishlistId
        };

        // Mock the StartPersonalWishlistAsync method to return the expected wishlist
        _wishListServiceMock.Setup(x => x.StartPersonalWishlistAsync(It.IsAny<WishlistCreateDto>(), CancellationToken.None))
            .ReturnsAsync(new WishlistDto
            {
                Id = "123",
                Name = "MacBook",
                Type = WishlistTypes.Product.ToString(), // Use enum
                CreatedById = "someId"
            });

        // Mock the GetChatCompletionStream method to provide SSE data
        var sseData = new List<string> { "[Question] What is your question?" };
        _openAiServiceMock.Setup(x => x.GetChatCompletionStream(It.IsAny<ChatCompletionRequest>(), cancellationToken))
            .Returns(sseData.ToAsyncEnumerable());

        // Act
        var result = await _productService.SearchProductAsync(wishlistId, message, cancellationToken).ToListAsync();

        // Assert
        // Check if the first item in the result is the expected wishlist creation event
        var firstEvent = result.FirstOrDefault();
        Assert.NotNull(firstEvent);
        Assert.Equal(expectedEvent.Event, firstEvent.Event);
        Assert.Equal(expectedEvent.Data, firstEvent.Data);

        // You can add more assertions to verify the other SSE events as needed.
    }*/
    
    
    [Fact]
    public async Task SearchProductAsync_WhenWishlistExists_ReturnsExpectedEvents()
    {
        // Arrange
        string wishlistId = "existingWishlistId"; // Simulating an existing wishlist ID
        var message = new MessageCreateDto
        {
            Text = "Your message text here"
        };
        var cancellationToken = CancellationToken.None;

        // Define your expected SSE data for the test
        var expectedSseData = new List<string>
        {
            "[",
            "Question",
            "]",
            " What",
            " features",
            " are",
            " you",
            " looking",
            "?\n",
            "[",
            "Options",
            "]",
            " USB", 
            "-C"
        };

        // Mock the GetChatCompletionStream method to provide the expected SSE data
        _openAiServiceMock.Setup(x => x.GetChatCompletionStream(It.IsAny<ChatCompletionRequest>(), cancellationToken))
            .Returns(expectedSseData.ToAsyncEnumerable());

        // Act
        var resultStream = _productService.SearchProductAsync(wishlistId, message, cancellationToken);

        // Convert the result stream to a list of ServerSentEvent
        var actualSseEvents = await resultStream.ToListAsync();

        // Assert
        // Check if the actual SSE events match the expected SSE events
        Assert.Equal(8, actualSseEvents.Count);
    }
    
    [Fact]
    public async Task StartNewSearchAndReturnWishlist_CreatesWishlistObject()
    {
        // Arrange
        var expectedOpenAiMessage = new OpenAiMessage
        {
            Role = "User",
            Content = "{ \"Name\": [{ \"Name\": \"NVIDIA GeForce RTX 3080\" }, { \"Name\": \"AMD Radeon RX 6900 XT\" }] }"
        };
        
        _openAiServiceMock.Setup(x => x.GetChatCompletionStream(It.IsAny<ChatCompletionRequest>(), CancellationToken.None))
            .Returns((ChatCompletionRequest request, CancellationToken token) =>
            {
                var asyncEnumerable = new List<string> { expectedOpenAiMessage.Content }.ToAsyncEnumerable();
                return asyncEnumerable;
            });
    
        _wishListServiceMock.Setup(x => x.StartPersonalWishlistAsync(It.IsAny<WishlistCreateDto>(), CancellationToken.None))
            .ReturnsAsync(new WishlistDto
            {
                Id = "someID",
                Name = "MacBook",
                Type = "Product", // Use enum
                CreatedById = "someId"
            });

        var message = new Message
        {
            Id = ObjectId.Parse("ab6c2c2d9edf39abcd1ef9ab"),
            Text = "what are the best graphics cards you know?",
            CreatedById = ObjectId.Parse("ab6c2c2d9edf39abcd1ef9ab"),
            Role = "user"
        };

        List<ProductName> productNames = null;
        WishlistDto createdWishList = null;

        // Act
        var result = _productService.StartNewSearchAndReturnWishlist(message, CancellationToken.None);

        await foreach (var (productList, wishlist) in result)
        {
            productNames = productList;
            createdWishList = wishlist;
        }
        
        // Assert
        Assert.NotNull(createdWishList);
        Assert.NotNull(productNames);
    }
    
    [Fact]
    public async Task GetProductFromSearch_ReturnsProductListWithName()
    {
        var message = new Message
        {
            Id = ObjectId.Parse("ab6c2c2d9edf39abcd1ef9ab"),
            Text = "what are the best graphics cards you know?",
            CreatedById = ObjectId.Parse("ab6c2c2d9edf39abcd1ef9ab"),
            Role = "user"
        };
        var cancellationToken = CancellationToken.None;

        var expectedOpenAiMessage = new OpenAiMessage
        {
            Role = "User",
            Content = "{ \"Name\": [{ \"Name\": \"NVIDIA GeForce RTX 3080\" }, { \"Name\": \"AMD Radeon RX 6900 XT\" }] }"
        };

        _openAiServiceMock.Setup(x => x.GetChatCompletionStream(It.IsAny<ChatCompletionRequest>(), cancellationToken))
            .Returns(new List<string> { expectedOpenAiMessage.Content }.ToAsyncEnumerable());

        var productService = new ProductService(_openAiServiceMock.Object, _wishListServiceMock.Object);

        var productList = new List<string>();
    
        await foreach (var product in productService.GetProductFromSearch(message, cancellationToken))
        {
            productList.Add(product);
        }

        var openAiContent = JObject.Parse(expectedOpenAiMessage.Content);
        var productNames = openAiContent["Name"].ToObject<List<ProductName>>();
        var expectedProductList = productNames.Select(info => info.Name).ToList();
    
        Assert.Equal(expectedProductList, productList);
        Assert.NotNull(openAiContent);
        Assert.True(openAiContent.ContainsKey("Name"));
    }
    
    [Fact]
    public async Task GetProductFromSearch_ReturnsProductListWithQuestion()
    {
        var message = new Message
        {
            Id = ObjectId.Parse("ab6c2c2d9edf39abcd1ef9ab"),
            Text = "what are the best graphics cards you know?",
            CreatedById = ObjectId.Parse("ab6c2c2d9edf39abcd1ef9ab"),
            Role = "user"
        };
        var cancellationToken = CancellationToken.None;

        var expectedOpenAiMessage = new OpenAiMessage
        {
            Role = "User",
            Content = "{ \"AdditionalQuestion\": [{ \"QuestionText\": \"What specific MacBook model are you using?\" }," +
                      " { \"QuestionText\": \"Do you have any preferences for brand or capacity?\" }] }"
        };

        _openAiServiceMock.Setup(x => x.GetChatCompletionStream(It.IsAny<ChatCompletionRequest>(), cancellationToken))
            .Returns(new List<string> { expectedOpenAiMessage.Content }.ToAsyncEnumerable());

        var productService = new ProductService(_openAiServiceMock.Object, _wishListServiceMock.Object);

        var productList = new List<string>();
    
        await foreach (var product in productService.GetProductFromSearch(message, cancellationToken))
        {
            productList.Add(product);
        }

        var openAiContent = JObject.Parse(expectedOpenAiMessage.Content);
        var productNames = openAiContent["AdditionalQuestion"].ToObject<List<Question>>();
        
        Assert.NotNull(openAiContent);
        Assert.True(openAiContent.ContainsKey("AdditionalQuestion"));
    }
    
    [Fact]
    public async Task GetRecommendationsForProductFromSearch_ReturnsRecommendations()
    {
        var message = new Message
        {
            Id = ObjectId.Parse("ab6c2c2d9edf39abcd1ef9ab"),
            Text = "get recommendations for this product",
            CreatedById = ObjectId.Parse("ab6c2c2d9edf39abcd1ef9ab"),
            Role = "user"
        };
        var cancellationToken = CancellationToken.None;

        var expectedOpenAiMessage = new OpenAiMessage
        {
            Role = "User",
            Content = "{ \"Recommendation\": [\"Recommendation 1\", \"Recommendation 2\"] }"
        };
        
        _openAiServiceMock.Setup(x => x.GetChatCompletionStream(It.IsAny<ChatCompletionRequest>(), cancellationToken))
            .Returns((ChatCompletionRequest request, CancellationToken token) =>
            {
                var asyncEnumerable = new List<string> { expectedOpenAiMessage.Content }.ToAsyncEnumerable();
                return asyncEnumerable;
            });

        var recommendations = new List<string>();
        var productService = new ProductService(_openAiServiceMock.Object, _wishListServiceMock.Object);
        
        await foreach (var recommendation in productService.GetRecommendationsForProductFromSearchStream(message, cancellationToken))
        {
            recommendations.Add(recommendation);
        }

        var openAiContent = JObject.Parse(expectedOpenAiMessage.Content);
        Assert.NotNull(openAiContent);
        Assert.True(openAiContent.ContainsKey("Recommendation"));
        Assert.Equal(new List<string> { "Recommendation 1", "Recommendation 2" }, recommendations);
    }
}