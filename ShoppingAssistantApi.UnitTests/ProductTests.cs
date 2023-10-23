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
            "Message",
            "]",
            " What",
            " u",
            " want",
            " ?",
            "[",
            "Options",
            "]",
            " USB-C",
            " ;",
            " Keyboard",
            " ultra",
            " ;",
            "?\n",
            "[",
            "Options",
            "]",
            " USB", 
            "-C",
            " ;",
            "[",
            "Message",
            "]",
            " What",
            " u",
            " want",
            " ?"
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
}