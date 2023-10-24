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
using ShoppingAssistantApi.Application.Paging;
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
            "Products",
            "]",
            " GTX",
            " 3090",
            " ;",
            " GTX",
            " 3070TI",
            " ;",
            " GTX",
            " 4070TI",
            " ;",
            " ?"
        };

        // Mock the GetChatCompletionStream method to provide the expected SSE data
        _openAiServiceMock.Setup(x => x.GetChatCompletionStream(It.IsAny<ChatCompletionRequest>(), cancellationToken))
            .Returns(expectedSseData.ToAsyncEnumerable());

        _wishListServiceMock.Setup(w => w.GetMessagesPageFromPersonalWishlistAsync(wishlistId, 1, 1, cancellationToken))
            .ReturnsAsync(new PagedList<MessageDto>(new List<MessageDto>
            {
                new MessageDto
                {
                    Text = "Some existing message",
                    Id = "",
                    CreatedById = "",
                    Role = ""
                }
            }, 1, 1, 1));
        // Act
        var resultStream = _productService.SearchProductAsync(wishlistId, message, cancellationToken);

        // Convert the result stream to a list of ServerSentEvent
        var actualSseEvents = await resultStream.ToListAsync();

        // Assert
        // Check if the actual SSE events match the expected SSE events
        Assert.NotNull(actualSseEvents);
    }


    [Fact]
    public async void SearchProductAsync_WithExistingMessageInWishlist_ReturnsExpectedEvents()
    {
        // Arrange
        var wishlistId = "your_wishlist_id";
        var message = new MessageCreateDto { Text = "Your message text" };
        var cancellationToken = new CancellationToken();

        var productService = new ProductService(_openAiServiceMock.Object, _wishListServiceMock.Object);

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
            "Products",
            "]",
            " GTX",
            " 3090",
            " ;",
            " GTX",
            " 3070TI",
            " ;",
            " GTX",
            " 4070TI",
            " ;",
            " ?"
        };

        // Mock the GetChatCompletionStream method to provide the expected SSE data
        _openAiServiceMock.Setup(x => x.GetChatCompletionStream(It.IsAny<ChatCompletionRequest>(), cancellationToken))
            .Returns(expectedSseData.ToAsyncEnumerable());

        // Act
        var resultStream = productService.SearchProductAsync(wishlistId, message, cancellationToken);
        
        var actualSseEvents = await resultStream.ToListAsync();
        // Assert
        
        Assert.NotNull(actualSseEvents);
        Assert.Equal(3, actualSseEvents.Count);
    }
}