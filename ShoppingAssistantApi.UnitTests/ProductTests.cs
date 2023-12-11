using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Moq;
using ShoppingAssistantApi.Application.IRepositories;
using ShoppingAssistantApi.Application.IServices;
using ShoppingAssistantApi.Application.Models.CreateDtos;
using ShoppingAssistantApi.Application.Models.Dtos;
using ShoppingAssistantApi.Application.Models.OpenAi;
using ShoppingAssistantApi.Application.Paging;
using ShoppingAssistantApi.Domain.Entities;
using ShoppingAssistantApi.Domain.Enums;
using ShoppingAssistantApi.Infrastructure.Services;
using System.Linq.Expressions;

namespace ShoppingAssistantApi.Tests.Tests;

public class ProductTests
{
    private Mock<IOpenAiService> _openAiServiceMock;

    private IProductService _productService;

    private Mock<IWishlistsService> _wishListServiceMock;

    private Mock<IMessagesRepository> _messagesRepositoryMock;

    public ProductTests()
    {
        _messagesRepositoryMock = new Mock<IMessagesRepository>();
        _openAiServiceMock = new Mock<IOpenAiService>();
        _wishListServiceMock = new Mock<IWishlistsService>();
        _productService = new ProductService(_openAiServiceMock.Object, _wishListServiceMock.Object, _messagesRepositoryMock.Object, new Mock<ILogger<ProductService>>().Object);
    }
    
    [Fact]
    public async Task SearchProductAsync_WhenWishlistsWithoutMessages_ReturnsExpectedEvents()
    {
        // Arrange
        string wishlistId = "657657677c13ae4bc95e2f41";
        var message = new MessageCreateDto
        {
            Text = "Your message text here"
        };
        var cancellationToken = CancellationToken.None;

        // Define your expected SSE data for the test
        var expectedSseData = new List<string>
        {
            "[", "Message", "]", " What", " u", " want", " ?", "[", "Suggestions", "]", " USB-C", " ;", " Keyboard", " ultra", 
            " ;", "[", "Suggestions", "]", " USB", "-C", " ;", "[", "Products", "]", " GTX", " 3090", " ;", " GTX",
            " 3070TI", " ;", " GTX", " 4070TI", " ;", " ?", "[", "Message", "]", " What", " u", " want", " ?"
        };
        
        var expectedMessages = new List<string> { " What", " u", " want", " ?", " What", " u", " want", " ?" };
        var expectedSuggestion = new List<string> { "USB-C", "Keyboard ultra", "USB-C" };

        // Mock the GetChatCompletionStream method to provide the expected SSE data
        _openAiServiceMock.Setup(x => x.GetChatCompletionStream(It.IsAny<ChatCompletionRequest>(), cancellationToken))
            .Returns(expectedSseData.ToAsyncEnumerable());
        
        _messagesRepositoryMock.Setup(m => m.GetWishlistMessagesAsync(It.IsAny<ObjectId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Message>
                {
                    new() 
                    {
                        Text = "What are you looking for?",
                        Role = "User"
                    },
                });

        // Act
        var resultStream = _productService.SearchProductAsync(wishlistId, message, cancellationToken);

        // Convert the result stream to a list of ServerSentEvent
        var actualSseEvents = await resultStream.ToListAsync();

        var receivedMessages = actualSseEvents
            .Where(e => e.Event == SearchEventType.Message)
            .Select(e => e.Data)
            .ToList();

        var receivedSuggestions = actualSseEvents
            .Where(e => e.Event == SearchEventType.Suggestion)
            .Select(e => e.Data)
            .ToList();
        
        // Assert
        // Check if the actual SSE events match the expected SSE events
        Assert.NotNull(actualSseEvents);
        Assert.Equal(expectedMessages, receivedMessages);
        Assert.Equal(expectedSuggestion, receivedSuggestions);
    }


    [Fact]
    public async void SearchProductAsync_WithExistingMessageInWishlist_ReturnsExpectedEvents()
    {
        // Arrange
        var wishlistId = "657657677c13ae4bc95e2f41";
        var message = new MessageCreateDto { Text = "Your message text" };
        var cancellationToken = new CancellationToken();

        var productService = _productService;
        
        var expectedSseData = new List<string>
        {
            "[", "Message", "]", " What", " u", " want", " ?", "[", "Suggestions", "]", "USB-C", " ;", "Keyboard", " ultra", 
            " ;", "[", "Suggestions", "]", "USB", "-C", " ;"
        };
        
        var expectedMessages = new List<string> { " What", " u", " want", " ?" };
        var expectedSuggestions = new List<string> { "USB-C", "Keyboard ultra", "USB-C" };

        // Mock the GetChatCompletionStream method to provide the expected SSE data
        _openAiServiceMock.Setup(x => x.GetChatCompletionStream(It.IsAny<ChatCompletionRequest>(), cancellationToken))
            .Returns(expectedSseData.ToAsyncEnumerable());

        _messagesRepositoryMock.Setup(m => m.GetWishlistMessagesAsync(It.IsAny<ObjectId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Message>
                {
                   new() {
                        Text = "Message 1",
                        Role = "User"
                    },
                    new Message
                    {
                        Text = "Message 2",
                        Role = "User"
                    },
                    new Message
                    {
                        Text = "Message 3",
                        Role = "User"
                    },
                });
        
        // Act
        var resultStream = _productService.SearchProductAsync(wishlistId, message, cancellationToken);

        // Convert the result stream to a list of ServerSentEvent
        var actualSseEvents = await resultStream.ToListAsync();
        
        var receivedMessages = actualSseEvents
            .Where(e => e.Event == SearchEventType.Message)
            .Select(e => e.Data)
            .ToList();

        var receivedSuggestions = actualSseEvents
            .Where(e => e.Event == SearchEventType.Suggestion)
            .Select(e => e.Data)
            .ToList();
        // Assert
        
        Assert.NotNull(actualSseEvents);
        Assert.Equal(expectedMessages, receivedMessages);
        Assert.Equal(expectedSuggestions, receivedSuggestions);
    }
    
    
    [Fact]
    public async void SearchProductAsync_WithExistingMessageInWishlistAndAddProduct_ReturnsExpectedEvents()
    {
        // Arrange
        var wishlistId = "657657677c13ae4bc95e2f41";
        var message = new MessageCreateDto { Text = "Your message text" };
        var cancellationToken = new CancellationToken();

        var productService = _productService;
        
        var expectedSseData = new List<string>
        {
            "[", "Message", "]", " What", " u", " want", " ?", "[", "Suggestions", "]", "USB-C", " ;", "Keyboard", " ultra", 
            " ;", "[", "Suggestions", "]", "USB", "-C", " ;", "[", "Products", "]", " GTX", " 3090", " ;", " GTX",
            " 3070TI", " ;", " GTX", " 4070TI", " ;", " ?"
        };
        
        var expectedMessages = new List<string> { " What", " u", " want", " ?" };
        var expectedSuggestions = new List<string> { "USB-C", "Keyboard ultra", "USB-C" };

        // Mock the GetChatCompletionStream method to provide the expected SSE data
        _openAiServiceMock.Setup(x => x.GetChatCompletionStream(It.IsAny<ChatCompletionRequest>(), cancellationToken))
            .Returns(expectedSseData.ToAsyncEnumerable());
        
        _messagesRepositoryMock.Setup(m => m.GetWishlistMessagesAsync(It.IsAny<ObjectId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Message>
                {
                   new() 
                   {
                        Text = "Message 1",
                        Role = "User"
                    },
                    new() 
                    {
                        Text = "Message 2",
                        Role = "User"
                    },
                    new() 
                    {
                        Text = "Message 3",
                        Role = "User"
                    },
                });
        
        // Act
        var resultStream = _productService.SearchProductAsync(wishlistId, message, cancellationToken);

        // Convert the result stream to a list of ServerSentEvent
        var actualSseEvents = await resultStream.ToListAsync();
        
        var receivedMessages = actualSseEvents
            .Where(e => e.Event == SearchEventType.Message)
            .Select(e => e.Data)
            .ToList();

        var receivedSuggestions = actualSseEvents
            .Where(e => e.Event == SearchEventType.Suggestion)
            .Select(e => e.Data)
            .ToList();
        
        // Assert
        Assert.NotNull(actualSseEvents);
        Assert.Equal(expectedMessages, receivedMessages);
        Assert.Equal(expectedSuggestions, receivedSuggestions);
    }
}