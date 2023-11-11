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
        _productService = new ProductService(_openAiServiceMock.Object, _wishListServiceMock.Object, _messagesRepositoryMock.Object);
    }
    
    [Fact]
    public async Task SearchProductAsync_WhenWishlistsWithoutMessages_ReturnsExpectedEvents()
    {
        // Arrange
        string wishlistId = "existingWishlistId";
        var message = new MessageCreateDto
        {
            Text = "Your message text here"
        };
        var cancellationToken = CancellationToken.None;

        // Define your expected SSE data for the test
        var expectedSseData = new List<string>
        {
            "[", "Message", "]", " What", " u", " want", " ?", "[", "Options", "]", " USB-C", " ;", " Keyboard", " ultra", 
            " ;", "[", "Options", "]", " USB", "-C", " ;", "[", "Products", "]", " GTX", " 3090", " ;", " GTX",
            " 3070TI", " ;", " GTX", " 4070TI", " ;", " ?", "[", "Message", "]", " What", " u", " want", " ?"
        };
        
        var expectedMessages = new List<string> { " What", " u", " want", " ?", " What", " u", " want", " ?" };
        var expectedSuggestion = new List<string> { " USB-C", " Keyboard ultra", " USB-C" };

        // Mock the GetChatCompletionStream method to provide the expected SSE data
        _openAiServiceMock.Setup(x => x.GetChatCompletionStream(It.IsAny<ChatCompletionRequest>(), cancellationToken))
            .Returns(expectedSseData.ToAsyncEnumerable());
        
        _messagesRepositoryMock.Setup(m => m.GetCountAsync(It.IsAny<Expression<Func<Message, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        
        _wishListServiceMock.Setup(w => w.AddMessageToPersonalWishlistAsync(wishlistId, It.IsAny<MessageCreateDto>(), cancellationToken))
            .Verifiable();
        
        _wishListServiceMock
            .Setup(m => m.GetMessagesPageFromPersonalWishlistAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()) 
            )
            .ReturnsAsync(new PagedList<MessageDto>(
                new List<MessageDto>
                {
                    new MessageDto
                    {
                        Text = "What are you looking for?",
                        Id = "3",
                        CreatedById = "User2",
                        Role = "User"
                    },
                },
                1,
                1,
                1
            ));
        
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
        var wishlistId = "your_wishlist_id";
        var message = new MessageCreateDto { Text = "Your message text" };
        var cancellationToken = new CancellationToken();

        var productService = _productService;
        
        var expectedSseData = new List<string>
        {
            "[", "Message", "]", " What", " u", " want", " ?", "[", "Options", "]", "USB-C", " ;", "Keyboard", " ultra", 
            " ;", "[", "Options", "]", "USB", "-C", " ;"
        };
        
        var expectedMessages = new List<string> { " What", " u", " want", " ?" };
        var expectedSuggestions = new List<string> { "USB-C", "Keyboard ultra", "USB-C" };

        // Mock the GetChatCompletionStream method to provide the expected SSE data
        _openAiServiceMock.Setup(x => x.GetChatCompletionStream(It.IsAny<ChatCompletionRequest>(), cancellationToken))
            .Returns(expectedSseData.ToAsyncEnumerable());
        
        _messagesRepositoryMock.Setup(m => m.GetCountAsync(It.IsAny<Expression<Func<Message, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        _wishListServiceMock.Setup(w => w.AddMessageToPersonalWishlistAsync(wishlistId, It.IsAny<MessageCreateDto>(), cancellationToken))
            .Verifiable();
        
        _wishListServiceMock
            .Setup(w => w.GetMessagesPageFromPersonalWishlistAsync(
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<MessageDto>(new List<MessageDto>
            {
                new MessageDto
                {
                    Text = "Message 1",
                    Id = "1",
                    CreatedById = "User2",
                    Role = "User"
                },
                new MessageDto
                {
                    Text = "Message 2",
                    Id = "2",
                    CreatedById = "User2",
                    Role = "User"
                },
                new MessageDto
                {
                    Text = "Message 3",
                    Id = "3",
                    CreatedById = "User2",
                    Role = "User"
                },
            }, 1, 3, 3));
        
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
        _wishListServiceMock.Verify(w => w.AddMessageToPersonalWishlistAsync(wishlistId, It.IsAny<MessageCreateDto>(), cancellationToken), Times.Once);
    }
    
    
    [Fact]
    public async void SearchProductAsync_WithExistingMessageInWishlistAndAddProduct_ReturnsExpectedEvents()
    {
        // Arrange
        var wishlistId = "your_wishlist_id";
        var message = new MessageCreateDto { Text = "Your message text" };
        var cancellationToken = new CancellationToken();

        var productService = _productService;
        
        var expectedSseData = new List<string>
        {
            "[", "Message", "]", " What", " u", " want", " ?", "[", "Options", "]", "USB-C", " ;", "Keyboard", " ultra", 
            " ;", "[", "Options", "]", "USB", "-C", " ;", "[", "Products", "]", " GTX", " 3090", " ;", " GTX",
            " 3070TI", " ;", " GTX", " 4070TI", " ;", " ?"
        };
        
        var expectedMessages = new List<string> { " What", " u", " want", " ?" };
        var expectedSuggestions = new List<string> { "USB-C", "Keyboard ultra", "USB-C" };

        // Mock the GetChatCompletionStream method to provide the expected SSE data
        _openAiServiceMock.Setup(x => x.GetChatCompletionStream(It.IsAny<ChatCompletionRequest>(), cancellationToken))
            .Returns(expectedSseData.ToAsyncEnumerable());
        
        _messagesRepositoryMock.Setup(m => m.GetCountAsync(It.IsAny<Expression<Func<Message, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        _wishListServiceMock
            .Setup(w => w.AddProductToPersonalWishlistAsync(
                It.IsAny<string>(), It.IsAny<ProductCreateDto>(), It.IsAny<CancellationToken>()))
            .Verifiable();
        
        _wishListServiceMock.Setup(w => w.AddProductToPersonalWishlistAsync(wishlistId, It.IsAny<ProductCreateDto>(), cancellationToken))
            .Verifiable();
        
        _wishListServiceMock
            .Setup(w => w.GetMessagesPageFromPersonalWishlistAsync(
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<MessageDto>(new List<MessageDto>
            {
                new MessageDto
                {
                    Text = "Message 1",
                    Id = "1",
                    CreatedById = "User2",
                    Role = "User"
                },
                new MessageDto
                {
                    Text = "Message 2",
                    Id = "2",
                    CreatedById = "User2",
                    Role = "User"
                },
                new MessageDto
                {
                    Text = "Message 3",
                    Id = "3",
                    CreatedById = "User2",
                    Role = "User"
                },
            }, 1, 3, 3));
        
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
        _wishListServiceMock.Verify(w => w.AddProductToPersonalWishlistAsync(
             It.IsAny<string>(), It.IsAny<ProductCreateDto>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
        _wishListServiceMock.Verify(w => w.AddMessageToPersonalWishlistAsync(
            wishlistId, It.IsAny<MessageCreateDto>(), cancellationToken), Times.Once);
    }
}