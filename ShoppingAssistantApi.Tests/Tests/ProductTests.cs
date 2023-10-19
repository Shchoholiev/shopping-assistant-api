using MongoDB.Bson;
using Moq;
using Newtonsoft.Json.Linq;
using ShoppingAssistantApi.Application.IServices;
using ShoppingAssistantApi.Application.Models.OpenAi;
using ShoppingAssistantApi.Application.Models.ProductSearch;
using ShoppingAssistantApi.Domain.Entities;
using ShoppingAssistantApi.Domain.Enums;
using ShoppingAssistantApi.Tests.TestExtentions;

namespace ShoppingAssistantApi.Tests.Tests;

public class ProductTests : TestsBase
{
    private Mock<IOpenAiService> _openAiServiceMock;
    
    private Mock<IProductService> _productServiceMock;
    
    public ProductTests(TestingFactory<Program> factory) : base(factory)
    {
        _openAiServiceMock = new Mock<IOpenAiService>();
        _productServiceMock = new Mock<IProductService>();
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
        
        var expectedProductList = new List<string> { "NVIDIA GeForce RTX 3080", "AMD Radeon RX 6900 XT" };
        _productServiceMock.Setup(x => x.GetProductFromSearch(message, cancellationToken))
            .ReturnsAsync(expectedProductList);

        Wishlist createdWishList = null;
        var expectedOpenAiMessage = new OpenAiMessage
        {
            Role = OpenAiRole.User,
            Content = "{ \"Name\": [{ \"Name\": \"NVIDIA GeForce RTX 3080\" }, { \"Name\": \"AMD Radeon RX 6900 XT\" }] }"
        };
        _openAiServiceMock.Setup(x => x.GetChatCompletion(It.IsAny<ChatCompletionRequest>(), cancellationToken))
            .ReturnsAsync(expectedOpenAiMessage);
        _productServiceMock
            .Setup(x => x.StartNewSearchAndReturnWishlist(It.IsAny<Message>(), cancellationToken))
            .ReturnsAsync(() =>
            {
                createdWishList = new Wishlist
                {
                    Name = "Test Wishlist",
                    CreatedById = ObjectId.GenerateNewId(),
                    Id = ObjectId.GenerateNewId(),
                    Type = "Test Type"
                };
                return new List<string>();
            });
    
        await _productServiceMock.Object.StartNewSearchAndReturnWishlist(message, cancellationToken);
   
        var openAiContent = JObject.Parse(expectedOpenAiMessage.Content);
        var productNames = openAiContent["Name"].ToObject<List<ProductName>>();
        var productList = productNames.Select(info => info.Name).ToList();

        Assert.Equal(expectedProductList, productList);
        Assert.True(openAiContent.ContainsKey("Name"));
        Assert.NotNull(createdWishList);
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