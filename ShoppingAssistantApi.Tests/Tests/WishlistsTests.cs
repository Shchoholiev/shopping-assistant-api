using ShoppingAssistantApi.Tests.TestExtentions;
using ShoppingAssistantApi.Domain.Enums;
using ShoppingAssistantApi.Application.Models.Dtos;
using ShoppingAssistantApi.Application.Paging;
using Newtonsoft.Json.Linq;

namespace ShoppingAssistantApi.Tests.Tests;

// TODO: make errors test more descrptive
public class WishlistsTests : TestsBase
{
    // From DbInitializer
    private const string TestingUserId = "652c3b89ae02a3135d6418fc";

    private const string TestingUserEmail = "wishlists@gmail.com";

    private const string TestingUserPassword = "Yuiop12345";

    private const string TestingWishlistId = "ab79cde6f69abcd3efab65cd";

    public WishlistsTests(TestingFactory<Program> factory)
        : base(factory)
    { }

    [Fact]
    public async Task StartPersonalWishlistAsync_ValidWishlist_ReturnsNewWishlist()
    {
        await LoginAsync(TestingUserEmail, TestingUserPassword);
        var mutation = new
        {
            query = @"
                mutation startPersonalWishlist($dto: WishlistCreateDtoInput!) { 
                    startPersonalWishlist (dto: $dto) { 
                        id, name, type, createdById 
                    } 
                }",
            variables = new
            {
                dto = new
                {
                    firstMessageText = "First message",
                    type = WishlistTypes.Product.ToString()
                }
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var wishlist = (WishlistDto?) jsonObject?.data?.startPersonalWishlist?.ToObject<WishlistDto>();

        Assert.NotNull(wishlist);
        Assert.Equal(TestingUserId, wishlist.CreatedById); 
        Assert.Equal(WishlistTypes.Product.ToString(), wishlist.Type);
        Assert.Equal($"{WishlistTypes.Product} Search", wishlist.Name);
    }

    [Fact]
    public async Task GetPersonalWishlistsPage_ValidPageNumberAndSize_ReturnsPage()
    {
        await LoginAsync(TestingUserEmail, TestingUserPassword);
        var query = new
        {
            query = @"
                query personalWishlistsPage($pageNumber: Int!, $pageSize: Int!) { 
                    personalWishlistsPage(pageNumber: $pageNumber, pageSize: $pageSize) { 
                        items { createdById, id, name, type } 
                    } 
                }",
            variables = new
            {
                pageNumber = 1,
                pageSize = 1
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(query);
        var pagedList = (PagedList<WishlistDto>?) jsonObject?.data?.personalWishlistsPage?.ToObject<PagedList<WishlistDto>>();

        Assert.NotNull(pagedList);
        Assert.NotEmpty(pagedList.Items);
    }

    [Fact]
    public async Task GetPersonalWishlist_ValidWishlistIdOrAuthorizedAccess_ReturnsWishlist()
    {
        await LoginAsync(TestingUserEmail, TestingUserPassword);
        var query = new
        {
            query = @"
                query personalWishlist($wishlistId: String!) { 
                    personalWishlist(wishlistId: $wishlistId) { 
                        createdById, id, name, type 
                    } 
                }",
            variables = new
            {
                wishlistId = TestingWishlistId
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(query);
        var wishlist = (WishlistDto?) jsonObject?.data?.personalWishlist?.ToObject<WishlistDto>();

        Assert.NotNull(wishlist);
        Assert.Equal("Gaming PC", wishlist.Name);
        Assert.Equal(WishlistTypes.Product.ToString(), wishlist.Type);
        Assert.Equal(TestingUserId, wishlist.CreatedById); 
    }

    [Fact]
    public async Task AddMessageToPersonalWishlist_ValidMessage_ReturnsNewMessage()
    {
        await LoginAsync(TestingUserEmail, TestingUserPassword);
        const string MessageText = "Second Message";
        var mutation = new
        {
            query = @"
                mutation addMessageToPersonalWishlist($wishlistId: String!, $dto: MessageCreateDtoInput!) { 
                    addMessageToPersonalWishlist(wishlistId: $wishlistId, dto: $dto) { 
                        role, text, createdById 
                    } 
                }",
            variables = new
            {
                wishlistId = TestingWishlistId,
                dto = new
                {
                    text = MessageText
                }
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var message = (MessageDto?) jsonObject?.data?.addMessageToPersonalWishlist?.ToObject<MessageDto>();

        Assert.NotNull(message);
        Assert.Equal(MessageRoles.User.ToString(), message.Role);
        Assert.Equal(MessageText, message.Text);
        Assert.Equal(TestingUserId, message.CreatedById);
    }

    [Fact]
    public async Task GetMessagesPageFromPersonalWishlist_ValidPageNumberAndSize_ReturnsPage()
    {
        await LoginAsync(TestingUserEmail, TestingUserPassword);
        var mutation = new
        {
            query = @"
                query messagesPageFromPersonalWishlist($wishlistId: String!, $pageNumber: Int!, $pageSize: Int!) { 
                    messagesPageFromPersonalWishlist (wishlistId: $wishlistId, pageNumber: $pageNumber, pageSize: $pageSize) { 
                        hasNextPage, 
                        hasPreviousPage, 
                        items { id, text, role, createdById }, 
                        pageNumber, 
                        pageSize, 
                        totalItems, 
                        totalPages 
                    } 
                }",
            variables = new
            {
                wishlistId = "ab79cde6f69abcd3efab95cd", // From DbInitializer 
                pageNumber = 1,
                pageSize = 2
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var pagedList = (PagedList<MessageDto>?) jsonObject?.data?.messagesPageFromPersonalWishlist?.ToObject<PagedList<MessageDto>>();
        
        Assert.NotNull(pagedList);
        Assert.NotEmpty(pagedList.Items);
        Assert.Equal("Third Message", pagedList.Items.FirstOrDefault()?.Text);
        Assert.Equal(MessageRoles.User.ToString(), pagedList.Items.FirstOrDefault()?.Role);
    }

    [Fact]
    public async Task StartPersonalWishlistAsync_InvalidWishlist_ReturnsErrors()
    {
        await LoginAsync(TestingUserEmail, TestingUserPassword);
        var mutation = new
        {
            query = @"
                mutation startPersonalWishlist($dto: WishlistCreateDtoInput!) { 
                    startPersonalWishlist (dto: $dto) { 
                        id, name, type, createdById 
                    } 
                }",
            variables = new
            {
                dto = new
                {
                    firstMessageText = "First message",
                    type = "Invalid type" // Invalid Wishlist Type
                }
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var errors = (JArray?) jsonObject?.errors;

        Assert.NotNull(errors);
        Assert.True(errors.Count > 0);
    }

    [Fact]
    public async Task GetPersonalWishlist_InvalidWishlistId_ReturnsErrors()
    {
        await LoginAsync(TestingUserEmail, TestingUserPassword);
        var query = new
        {
            query = @"
                query personalWishlist($wishlistId: String!) { 
                    personalWishlist(wishlistId: $wishlistId) { 
                        createdById, id, name, type 
                    } 
                }",
            variables = new
            {
                wishlistId = "1234567890abcdef12345678" // Invalid wishlistId
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(query);
        var errors = (JArray?) jsonObject?.errors;

        Assert.NotNull(errors);
        Assert.True(errors.Count > 0);
    }

    [Fact]
    public async Task GetPersonalWishlist_UnauthorizedAccess_ReturnsErrors()
    {
        await LoginAsync(TestingUserEmail, TestingUserPassword);
        var query = new
        {
            query = @"
                query personalWishlist($wishlistId: String!) { 
                    personalWishlist(wishlistId: $wishlistId) { 
                        createdById, id, name, type 
                    } 
                }",
            variables = new
            {
                wishlistId = "ab6c2c2d9edf39abcd1ef9ab" // Other user's wishlist
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(query);
        var errors = (JArray?) jsonObject?.errors;

        Assert.NotNull(errors);
        Assert.True(errors.Count > 0);
    }

    [Fact]
    public async Task AddMessageToPersonalWishlist_InvalidMessage_ReturnsErrors()
    {
        await LoginAsync(TestingUserEmail, TestingUserPassword);
        var mutation = new
        {
            query = @"
                mutation addMessageToPersonalWishlist($wishlistId: String!, $dto: MessageCreateDtoInput!) { 
                    addMessageToPersonalWishlist (wishlistId: $wishlistId, dto: $dto) { 
                        role, text, createdById 
                    } 
                }",
            variables = new
            {
                wishlistId = "8125jad7g12", // Invalid wishlistId
                dto = new
                {
                    text = "random text",
                }
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var errors = (JArray?) jsonObject?.errors;

        Assert.NotNull(errors);
        Assert.True(errors.Count > 0);
    }
}