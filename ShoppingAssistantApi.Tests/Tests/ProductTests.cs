using System.Collections.ObjectModel;
using Microsoft.VisualBasic;
using MongoDB.Bson;
using Moq;
using Newtonsoft.Json;
using ShoppingAssistantApi.Application.Models.Dtos;
using ShoppingAssistantApi.Application.IServices;
using ShoppingAssistantApi.Application.Models.OpenAi;
using ShoppingAssistantApi.Application.Models.ProductSearch;
using ShoppingAssistantApi.Domain.Entities;
using ShoppingAssistantApi.Domain.Enums;
using ShoppingAssistantApi.Tests.TestExtentions;
using ShoppingAssistantApi.Infrastructure.Services;

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

        var productServiceMock = new Mock<IProductService>();
        var expectedProductList = new List<string> { "NVIDIA GeForce RTX 3080", "AMD Radeon RX 6900 XT" };
        productServiceMock.Setup(x => x.GetProductFromSearch(message, cancellationToken))
            .ReturnsAsync(expectedProductList);

        var openAiServiceMock = new Mock<IOpenAiService>();
        var expectedOpenAiMessage = new OpenAiMessage
        {
            Role = OpenAiRole.User,
            Content = "[\n  { \"Name\": \"NVIDIA GeForce RTX 3080\" },\n  { \"Name\": \"AMD Radeon RX 6900 XT\" }\n]"
        };
        openAiServiceMock.Setup(x => x.GetChatCompletion(It.IsAny<ChatCompletionRequest>(), cancellationToken))
            .ReturnsAsync(expectedOpenAiMessage);

        var productList = JsonConvert.DeserializeObject<List<ProductName>>(expectedOpenAiMessage.Content).Select(info => info.Name).ToList();

        Assert.Equal(expectedProductList, productList);
    }
    
}