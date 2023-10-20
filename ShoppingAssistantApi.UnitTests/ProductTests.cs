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

    private Mock<IProductService> _productServiceMock;

    public Mock<IWishlistsService> _wishListServiceMock;

    public ProductTests()
    {
        _openAiServiceMock = new Mock<IOpenAiService>();
        _productServiceMock = new Mock<IProductService>();
        _wishListServiceMock = new Mock<IWishlistsService>();
    }
    
    [Fact]
    public async Task StartNewSearchAndReturnWishlist_CreatesWishlistObject()
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
            Role = OpenAiRole.User,
            Content = "{ \"Name\": [{ \"Name\": \"NVIDIA GeForce RTX 3080\" }, { \"Name\": \"AMD Radeon RX 6900 XT\" }] }"
        };
        
        var openAiServiceMock = new Mock<IOpenAiService>();
        var wishlistsServiceMock = new Mock<IWishlistsService>();
        
        openAiServiceMock.Setup(x => x.GetChatCompletionStream(It.IsAny<ChatCompletionRequest>(), cancellationToken))
            .Returns((ChatCompletionRequest request, CancellationToken token) =>
            {
                var asyncEnumerable = new List<string> { expectedOpenAiMessage.Content }.ToAsyncEnumerable();
                return asyncEnumerable;
            });
    
        wishlistsServiceMock.Setup(x => x.StartPersonalWishlistAsync(It.IsAny<WishlistCreateDto>(), cancellationToken))
            .ReturnsAsync(new WishlistDto
            {
                Id = "someID",
                Name = "MacBook",
                Type = "Product",
                CreatedById = "someId"
            });
        
        var productService = new ProductService(openAiServiceMock.Object, wishlistsServiceMock.Object);

        List<ProductName> productNames = null;
        WishlistDto createdWishList = null;

        var result = productService.StartNewSearchAndReturnWishlist(message, cancellationToken);

        await foreach (var (productList, wishlist) in result)
        {
            productNames = productList;
            createdWishList = wishlist;
        }

        var openAiContent = JObject.Parse(expectedOpenAiMessage.Content);

        Assert.True(openAiContent.ContainsKey("Name"));
        Assert.NotNull(createdWishList);
        Assert.NotNull(productNames);
    }
    
    [Fact]
    public async Task GetProductFromSearch_ReturnsProductList()
    {
        var message = new Message
        {
            Id = ObjectId.Parse("ab6c2c2d9edf39abcd1ef9ab"),
            Text = "what are the best graphics cards you know?",
            CreatedById = ObjectId.Parse("ab6c2c2d9edf39abcd1ef9ab"),
            Role = "user"
        };
        var cancellationToken = CancellationToken.None;
        
        var expectedProductList = new List<string> { "NVIDIA GeForce RTX 3080", "AMD Radeon RX 6900 XT" };
        _productServiceMock.Setup(x => x.GetProductFromSearch(message, cancellationToken))
            .ReturnsAsync(expectedProductList);
        
        var expectedOpenAiMessage = new OpenAiMessage
        {
            Role = OpenAiRole.User,
            Content = "{ \"Name\": [{ \"Name\": \"NVIDIA GeForce RTX 3080\" }, { \"Name\": \"AMD Radeon RX 6900 XT\" }] }"
        };
        _openAiServiceMock.Setup(x => x.GetChatCompletion(It.IsAny<ChatCompletionRequest>(), cancellationToken))
            .ReturnsAsync(expectedOpenAiMessage);

        var openAiContent = JObject.Parse(expectedOpenAiMessage.Content);
        var productNames = openAiContent["Name"].ToObject<List<ProductName>>();
        var productList = productNames.Select(info => info.Name).ToList();
        
        Assert.Equal(expectedProductList, productList);
        Assert.True(openAiContent.ContainsKey("Name"));
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
            Role = OpenAiRole.User,
            Content = "{ \"Recommendation\": [\"Recommendation 1\", \"Recommendation 2\"] }"
        };
        _openAiServiceMock.Setup(x => x.GetChatCompletion(It.IsAny<ChatCompletionRequest>(), cancellationToken))
            .ReturnsAsync(expectedOpenAiMessage);
        
        var recommendations = await _productServiceMock.Object.GetRecommendationsForProductFromSearch(message, cancellationToken);
        
        var openAiContent = JObject.Parse(expectedOpenAiMessage.Content);
        Assert.NotNull(openAiContent);
        Assert.True(openAiContent.ContainsKey("Recommendation"));
        
    }
}