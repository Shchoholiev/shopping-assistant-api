using ShoppingAssistantApi.Tests.TestExtentions;
using ShoppingAssistantApi.Domain.Enums;
using ShoppingAssistantApi.Application.Models.Dtos;
using ShoppingAssistantApi.Application.Paging;
using Newtonsoft.Json.Linq;

namespace ShoppingAssistantApi.Tests.Tests;

public class WishlistsTests : TestsBase
{
    // From DbInitializer
    private const string TestingUserId = "652c3b89ae02a3135d6418fc";

    private const string TestingUserEmail = "wishlists@gmail.com";

    private const string TestingUserPassword = "Yuiop12345";

    private const string TestingNotExistingWishlistId = "1234567890abcdef12345678";

    private const string TestingValidWishlistName = "Gaming PC";

    private const WishlistTypes TestingValidWishlistType = WishlistTypes.Product;

    private const string TestingUnauthorizedWishlistId = "ab6c2c2d9edf39abcd1ef9ab";

    private const string TestingValidWishlistId = "ab79cde6f69abcd3efab65cd";


    public WishlistsTests(TestingFactory<Program> factory)
        : base(factory)
    { }

    [Fact]
    public async Task StartPersonalWishlist_ValidWishlist_ReturnsNewWishlist()
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
    public async Task GenerateNameForPersonalWishlist_ValidWishlistId_ReturnsNewName()
    {
        await LoginAsync(TestingUserEmail, TestingUserPassword);
        var startWishlistMutation = new
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
                    firstMessageText = "Mechanical keyboard for programming",
                    type = WishlistTypes.Product.ToString()
                }
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(startWishlistMutation);
        var startWishlistResponse = (WishlistDto?) jsonObject?.data?.startPersonalWishlist?.ToObject<WishlistDto>();

        Assert.NotNull(startWishlistResponse);

        var generateWishlistNameMutation = new
        {
            query = @"
                mutation genarateNameForPersonalWishlist($wishlistId: String!) {
                    generateNameForPersonalWishlist(wishlistId: $wishlistId) {
                        id, name, type, createdById
                    }
                }",
            variables = new
            {
                wishlistId = startWishlistResponse.Id
            }
        };

        jsonObject = await SendGraphQlRequestAsync(generateWishlistNameMutation);
        var generateWishlistNameResponse = (WishlistDto?) jsonObject?.data?.generateNameForPersonalWishlist?.ToObject<WishlistDto>();

        Assert.NotNull(generateWishlistNameResponse);
        Assert.Equal(startWishlistResponse.Id, generateWishlistNameResponse.Id);

        Assert.NotEqual($"{startWishlistResponse.Type} Search", generateWishlistNameResponse.Name);
        Assert.NotEqual(String.Empty, generateWishlistNameResponse.Name);
        Assert.NotEqual(null, generateWishlistNameResponse.Name);
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
                wishlistId = TestingValidWishlistId
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(query);
        var wishlist = (WishlistDto?) jsonObject?.data?.personalWishlist?.ToObject<WishlistDto>();

        Assert.NotNull(wishlist);
        Assert.Equal(TestingValidWishlistName, wishlist.Name);
        Assert.Equal(TestingValidWishlistType.ToString(), wishlist.Type);
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
                wishlistId = TestingValidWishlistId,
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
    public async Task GetMessagesPageFromPersonalWishlist_ValidPageNumberAndSizeValidWishlistIdOrAuthorizedAccess_ReturnsPage()
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
                wishlistId = TestingValidWishlistId,
                pageNumber = 1,
                pageSize = 2
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var pagedList = (PagedList<MessageDto>?) jsonObject?.data?.messagesPageFromPersonalWishlist?.ToObject<PagedList<MessageDto>>();
        
        Assert.NotNull(pagedList);
        Assert.NotEmpty(pagedList.Items);
        Assert.Equal("Message 6", pagedList.Items.FirstOrDefault()?.Text);
        Assert.Equal(MessageRoles.Application.ToString(), pagedList.Items.FirstOrDefault()?.Role);
    }

    [Fact]
    public async Task AddProductToPersonalWishlist_ValidProduct_ReturnsNewProduct()
    {
        await LoginAsync(TestingUserEmail, TestingUserPassword);
        var mutation = new
        {
            query = @"
                mutation addProductToPersonalWishlist($wishlistId: String!, $dto: ProductCreateDtoInput!) {
                    addProductToPersonalWishlist (wishlistId: $wishlistId, dto: $dto) {
                        url, name, price, description, rating, imagesUrls, wasOpened
                    } 
                }",
            variables = new
            {
                wishlistId = TestingValidWishlistId,
                dto = new
                {
                    url = "https://www.amazon.com/url",
                    name = "Generic name",
                    description = "Generic description",
                    rating = 4.8,
                    price = 1,
                    imagesUrls = new string[]
                    {
                        "https://www.amazon.com/image-url-1",
                        "https://www.amazon.com/image-url-2"
                    },
                    wasOpened = false
                }
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var product = (ProductDto?) jsonObject?.data?.addProductToPersonalWishlist?.ToObject<ProductDto>();

        Assert.NotNull(product);
        Assert.Equal("https://www.amazon.com/url", product.Url);
        Assert.Equal("Generic name", product.Name);
        Assert.Equal("Generic description", product.Description);
        Assert.Equal(4.8, product.Rating);
        Assert.Equal("https://www.amazon.com/image-url-1", product.ImagesUrls[0]);
    }

    [Fact]
    public async Task GetProductsPageFromPersonalWishlist_ValidPageNumberAndSizeValidWishlistIdOrAuthorizedAccess_ReturnsPage()
    {
        await LoginAsync(TestingUserEmail, TestingUserPassword);
        var mutation = new
        {
            query = @"
                query productsPageFromPersonalWishlist($wishlistId: String!, $pageNumber: Int!, $pageSize: Int!) {
                    productsPageFromPersonalWishlist (wishlistId: $wishlistId, pageNumber: $pageNumber, pageSize: $pageSize) {
                        hasNextPage,
                        hasPreviousPage,
                        items { id, url, name, description, rating, imagesUrls, wasOpened, wishlistId },
                        pageNumber,
                        pageSize,
                        totalItems,
                        totalPages
                    }
                }",
            variables = new
            {
                wishlistId = TestingValidWishlistId,
                pageNumber = 1,
                pageSize = 2
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var pagedList = (PagedList<ProductDto>?) jsonObject?.data?.productsPageFromPersonalWishlist?.ToObject<PagedList<ProductDto>>();

        Assert.NotNull(pagedList);
        Assert.Equal("Samsung 970 EVO Plus SSD 2TB NVMe M.2 Internal Solid State Hard Drive, V-NAND Technology, Storage and Memory Expansion for Gaming, Graphics w/ Heat Control, Max Speed, MZ-V7S2T0B/AM ", pagedList.Items.LastOrDefault()?.Name);
        Assert.Equal(TestingValidWishlistId, pagedList.Items.LastOrDefault()?.WishlistId);
    }

    [Fact]
    public async Task DeletePersonalWishlist_ValidWishlistIdOrAuthorizedAccess_ReturnsWishlist()
    {
        await LoginAsync(TestingUserEmail, TestingUserPassword);
        var mutation = new
        {
            query = @"
                mutation deletePersonalWishlist($wishlistId: String!) {
                    deletePersonalWishlist (wishlistId: $wishlistId) {
                        createdById, id, name, type
                    }
                }",
            variables = new
            {
                wishlistId = TestingValidWishlistId
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var wishlist = (WishlistDto?) jsonObject?.data?.deletePersonalWishlist?.ToObject<WishlistDto>();

        Assert.NotNull(wishlist);
        Assert.Equal(TestingValidWishlistId, wishlist.Id);
        Assert.Equal(TestingValidWishlistName, wishlist.Name);
        Assert.Equal(TestingValidWishlistType.ToString(), wishlist.Type);
        Assert.Equal(TestingUserId, wishlist.CreatedById);
    }

    [Fact]
    public async Task StartPersonalWishlist_InvalidWishlist_ReturnsErrors()
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
    public async Task GetPersonalWishlistsPage_PageNumberGreaterThanAvailablePages_ReturnsEmptyList()
    {
        await LoginAsync(TestingUserEmail, TestingUserPassword);
        var mutation = new
        {
            query = @"
                query personalWishlistsPage($pageNumber: Int!, $pageSize: Int!) {
                    personalWishlistsPage(pageNumber: $pageNumber, pageSize: $pageSize) {
                        items { createdById, id, name, type }
                    }
                }",
            variables = new
            {
                pageNumber = 100,
                pageSize = 2
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var pagedList = (PagedList<WishlistDto>?) jsonObject?.data?.personalWishlistsPage?.ToObject<PagedList<WishlistDto>>();

        Assert.NotNull(pagedList);
        Assert.Empty(pagedList.Items);
    }

    [Fact]
    public async Task GetPersonalWishlistsPage_PageNumberLessThan1_ReturnsError()
    {
        await LoginAsync(TestingUserEmail, TestingUserPassword);
        var mutation = new
        {
            query = @"
                query personalWishlistsPage($pageNumber: Int!, $pageSize: Int!) {
                    personalWishlistsPage(pageNumber: $pageNumber, pageSize: $pageSize) {
                        items { createdById, id, name, type }
                    }
                }",
            variables = new
            {
                pageNumber = 0,
                pageSize = 1
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var errors = (JArray?) jsonObject?.errors;

        Assert.NotNull(errors);
        Assert.True(errors.Count > 0);
    }

    [Fact]
    public async Task GetPersonalWishlistsPage_PageSizeGreaterThanAvailableEntities_ReturnsPage()
    {
        await LoginAsync(TestingUserEmail, TestingUserPassword);
        var mutation = new
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
                pageSize = 100
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var pagedList = (PagedList<WishlistDto>?) jsonObject?.data?.personalWishlistsPage?.ToObject<PagedList<WishlistDto>>();

        Assert.NotNull(pagedList);
        Assert.NotEmpty(pagedList.Items);
        Assert.Equal(TestingUserId, pagedList.Items.FirstOrDefault()?.CreatedById);
    }

    [Fact]
    public async Task GetPersonalWishlistsPage_PageSizeLessThan0_ReturnsPage()
    {
        await LoginAsync(TestingUserEmail, TestingUserPassword);
        var mutation = new
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
                pageSize = -1
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var pagedList = (PagedList<WishlistDto>?) jsonObject?.data?.personalWishlistsPage?.ToObject<PagedList<WishlistDto>>();

        Assert.NotNull(pagedList);
        Assert.NotEmpty(pagedList.Items);
        Assert.Equal(TestingUserId, pagedList.Items.FirstOrDefault()?.CreatedById);
    }

    [Fact]
    public async Task GetPersonalWishlist_NotExistingWishlistId_ReturnsErrors()
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
                wishlistId = TestingNotExistingWishlistId
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(query);
        var errors = (JArray?) jsonObject?.errors;

        Assert.NotNull(errors);
        Assert.True(errors.Count > 0);
    }

    [Fact]
    public async Task GetMessagesPageFromPersonalWishlist_PageNumberGreaterThanAvailablePages_ReturnsEmptyList()
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
                wishlistId = TestingValidWishlistId,
                pageNumber = 100,
                pageSize = 2
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var pagedList = (PagedList<MessageDto>?) jsonObject?.data?.messagesPageFromPersonalWishlist?.ToObject<PagedList<MessageDto>>();

        Assert.NotNull(pagedList);
        Assert.Empty(pagedList.Items);
    }

    [Fact]
    public async Task GetMessagesPageFromPersonalWishlist_PageNumberLessThan1_ReturnsError()
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
                wishlistId = TestingValidWishlistId,
                pageNumber = 0,
                pageSize = 2
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var errors = (JArray?) jsonObject?.errors;

        Assert.NotNull(errors);
        Assert.True(errors.Count > 0);
    }

    [Fact]
    public async Task GetMessagesPageFromPersonalWishlist_PageSizeGreaterThanAvailableEntities_ReturnsPage()
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
                wishlistId = TestingValidWishlistId,
                pageNumber = 1,
                pageSize = 10
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var pagedList = (PagedList<MessageDto>?) jsonObject?.data?.messagesPageFromPersonalWishlist?.ToObject<PagedList<MessageDto>>();

        Assert.NotNull(pagedList);
        Assert.Equal("Message 6", pagedList.Items.FirstOrDefault()?.Text);
        Assert.Equal(MessageRoles.Application.ToString(), pagedList.Items.FirstOrDefault()?.Role);
    }

    [Fact]
    public async Task GetMessagesPageFromPersonalWishlist_PageSizeLessThan0_ReturnsPage()
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
                wishlistId = TestingValidWishlistId,
                pageNumber = 1,
                pageSize = -2
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var pagedList = (PagedList<MessageDto>?) jsonObject?.data?.messagesPageFromPersonalWishlist?.ToObject<PagedList<MessageDto>>();

        Assert.NotNull(pagedList);
        Assert.Equal("Message 6", pagedList.Items.FirstOrDefault()?.Text);
        Assert.Equal(MessageRoles.Application.ToString(), pagedList.Items.FirstOrDefault()?.Role);
    }

    [Fact]
    public async Task GetMessagesPageFromPersonalWishlist_NotExistingWishlistId_ReturnsError()
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
                wishlistId = TestingNotExistingWishlistId,
                pageNumber = 1,
                pageSize = 2
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var errors = (JArray?) jsonObject?.errors;

        Assert.NotNull(errors);
        Assert.True(errors.Count > 0);
    }

    [Fact]
    public async Task GetMessagesPageFromPersonalWishlist_OtherUserWishlistId_ReturnsError()
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
                wishlistId = TestingUnauthorizedWishlistId,
                pageNumber = 1,
                pageSize = 2
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var errors = (JArray?) jsonObject?.errors;

        Assert.NotNull(errors);
        Assert.True(errors.Count > 0);
    }

    [Fact]
    public async Task AddProductToPersonalWishlist_NotExistingWishlistId_RturnsError()
    {
        await LoginAsync(TestingUserEmail, TestingUserPassword);
        var mutation = new
        {
            query = @"
                mutation addProductToPersonalWishlist($wishlistId: String!, $dto: ProductCreateDtoInput!) {
                    addProductToPersonalWishlist (wishlistId: $wishlistId, dto: $dto) {
                        url, name, description, rating, imagesUrls, wasOpened
                    }
                }",
            variables = new
            {
                wishlistId = TestingNotExistingWishlistId,
                dto = new
                {
                    url = "https://www.amazon.com/url",
                    name = "Generic name",
                    description = "Generic description",
                    rating = 4.8,
                    imagesUrls = new string[]
                    {
                        "https://www.amazon.com/image-url-1",
                        "https://www.amazon.com/image-url-2"
                    },
                    wasOpened = false
                }
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var errors = (JArray?) jsonObject?.errors;

        Assert.NotNull(errors);
        Assert.True(errors.Count > 0);
    }

    [Fact]
    public async Task AddProductToPersonalWishlist_OtherUserWishlistId_RturnsError()
    {
        await LoginAsync(TestingUserEmail, TestingUserPassword);
        var mutation = new
        {
            query = @"
                mutation addProductToPersonalWishlist($wishlistId: String!, $dto: ProductCreateDtoInput!) {
                    addProductToPersonalWishlist (wishlistId: $wishlistId, dto: $dto) {
                        url, name, description, rating, imagesUrls, wasOpened
                    }
                }",
            variables = new
            {
                wishlistId = TestingUnauthorizedWishlistId,
                dto = new
                {
                    url = "https://www.amazon.com/url",
                    name = "Generic name",
                    description = "Generic description",
                    rating = 4.8,
                    imagesUrls = new string[]
                    {
                        "https://www.amazon.com/image-url-1",
                        "https://www.amazon.com/image-url-2"
                    },
                    wasOpened = false
                }
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var errors = (JArray?) jsonObject?.errors;

        Assert.NotNull(errors);
        Assert.True(errors.Count > 0);
    }

    [Fact]
    public async Task GetProductsPageFromPersonalWishlist_PageNumberGreaterThanAvailablePages_ReturnsEmptyList()
    {
        await LoginAsync(TestingUserEmail, TestingUserPassword);
        var mutation = new
        {
            query = @"
                query productsPageFromPersonalWishlist($wishlistId: String!, $pageNumber: Int!, $pageSize: Int!) {
                    productsPageFromPersonalWishlist (wishlistId: $wishlistId, pageNumber: $pageNumber, pageSize: $pageSize) {
                        hasNextPage,
                        hasPreviousPage,
                        items { id, url, name, description, rating, imagesUrls, wasOpened, wishlistId },
                        pageNumber,
                        pageSize,
                        totalItems,
                        totalPages
                    }
                }",
            variables = new
            {
                wishlistId = TestingValidWishlistId,
                pageNumber = 100,
                pageSize = 2
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var pagedList = (PagedList<WishlistDto>?) jsonObject?.data?.productsPageFromPersonalWishlist?.ToObject<PagedList<WishlistDto>>();

        Assert.NotNull(pagedList);
        Assert.Empty(pagedList.Items);
    }

    [Fact]
    public async Task GetProductsPageFromPersonalWishlist_PageNumberLessThan1_ReturnsError()
    {
        await LoginAsync(TestingUserEmail, TestingUserPassword);
        var mutation = new
        {
            query = @"
                query productsPageFromPersonalWishlist($wishlistId: String!, $pageNumber: Int!, $pageSize: Int!) {
                    productsPageFromPersonalWishlist (wishlistId: $wishlistId, pageNumber: $pageNumber, pageSize: $pageSize) {
                        hasNextPage,
                        hasPreviousPage,
                        items { id, url, name, description, rating, imagesUrls, wasOpened, wishlistId },
                        pageNumber,
                        pageSize,
                        totalItems,
                        totalPages
                    }
                }",
            variables = new
            {
                wishlistId = TestingValidWishlistId,
                pageNumber = 0,
                pageSize = 2
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var errors = (JArray?) jsonObject?.errors;

        Assert.NotNull(errors);
        Assert.True(errors.Count > 0);
    }

    [Fact]
    public async Task GetProductsPageFromPersonalWishlist_PageSizeGreaterThanAvailableEntities_ReturnsPage()
    {
        await LoginAsync(TestingUserEmail, TestingUserPassword);
        var mutation = new
        {
            query = "query productsPageFromPersonalWishlist($wishlistId: String!, $pageNumber: Int!, $pageSize: Int!) { productsPageFromPersonalWishlist (wishlistId: $wishlistId, pageNumber: $pageNumber, pageSize: $pageSize) { hasNextPage, hasPreviousPage, items { id, url, name, description, rating, imagesUrls, wasOpened, wishlistId }, pageNumber, pageSize, totalItems, totalPages } }",
            variables = new
            {
                wishlistId = TestingValidWishlistId,
                pageNumber = 1,
                pageSize = 100
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var pagedList = (PagedList<ProductDto>?) jsonObject?.data?.productsPageFromPersonalWishlist?.ToObject<PagedList<ProductDto>>();

        Assert.NotNull(pagedList);

        Assert.Equal("Samsung 970 EVO Plus SSD 2TB NVMe M.2 Internal Solid State Hard Drive, V-NAND Technology, Storage and Memory Expansion for Gaming, Graphics w/ Heat Control, Max Speed, MZ-V7S2T0B/AM ", pagedList.Items.ToList()[1].Name);
        Assert.Equal(TestingValidWishlistId, pagedList.Items.ToList()[1].WishlistId);
    }

    [Fact]
    public async Task GetProductsPageFromPersonalWishlist_PageSizeLessThan0_ReturnsPage()
    {
        await LoginAsync(TestingUserEmail, TestingUserPassword);
        var mutation = new
        {
            query = "query productsPageFromPersonalWishlist($wishlistId: String!, $pageNumber: Int!, $pageSize: Int!) { productsPageFromPersonalWishlist (wishlistId: $wishlistId, pageNumber: $pageNumber, pageSize: $pageSize) { hasNextPage, hasPreviousPage, items { id, url, name, description, rating, imagesUrls, wasOpened, wishlistId }, pageNumber, pageSize, totalItems, totalPages } }",
            variables = new
            {
                wishlistId = TestingValidWishlistId,
                pageNumber = 1,
                pageSize = -2
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var pagedList = (PagedList<ProductDto>?) jsonObject?.data?.productsPageFromPersonalWishlist?.ToObject<PagedList<ProductDto>>();

        Assert.NotNull(pagedList);

        Assert.Equal("Samsung 970 EVO Plus SSD 2TB NVMe M.2 Internal Solid State Hard Drive, V-NAND Technology, Storage and Memory Expansion for Gaming, Graphics w/ Heat Control, Max Speed, MZ-V7S2T0B/AM ", pagedList.Items.ToList()[1].Name);
        Assert.Equal(TestingValidWishlistId, pagedList.Items.ToList()[1].WishlistId);
    }

    [Fact]
    public async Task GetPersonalWishlist_OtherUserWishlistId_ReturnsErrors()
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
                wishlistId = TestingUnauthorizedWishlistId
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(query);
        var errors = (JArray?) jsonObject?.errors;

        Assert.NotNull(errors);
        Assert.True(errors.Count > 0);
    }

    [Fact]
    public async Task GetProductsPageFromPersonalWishlist_NotExistingWishlistId_ReturnsError()
    {
        await LoginAsync(TestingUserEmail, TestingUserPassword);
        var mutation = new
        {
            query = @"
                query productsPageFromPersonalWishlist($wishlistId: String!, $pageNumber: Int!, $pageSize: Int!) {
                    productsPageFromPersonalWishlist (wishlistId: $wishlistId, pageNumber: $pageNumber, pageSize: $pageSize) {
                        hasNextPage,
                        hasPreviousPage,
                        items { id, url, name, description, rating, imagesUrls, wasOpened, wishlistId },
                        pageNumber,
                        pageSize,
                        totalItems,
                        totalPages
                    }
                }",
            variables = new
            {
                wishlistId = TestingNotExistingWishlistId,
                pageNumber = 0,
                pageSize = 2
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var errors = (JArray?) jsonObject?.errors;

        Assert.NotNull(errors);
        Assert.True(errors.Count > 0);
    }

    [Fact]
    public async Task GetProductsPageFromPersonalWishlist_OtherUserWishlistId_ReturnsError()
    {
        await LoginAsync(TestingUserEmail, TestingUserPassword);
        var mutation = new
        {
            query = @"
                query productsPageFromPersonalWishlist($wishlistId: String!, $pageNumber: Int!, $pageSize: Int!) {
                    productsPageFromPersonalWishlist (wishlistId: $wishlistId, pageNumber: $pageNumber, pageSize: $pageSize) {
                        hasNextPage,
                        hasPreviousPage,
                        items { id, url, name, description, rating, imagesUrls, wasOpened, wishlistId },
                        pageNumber,
                        pageSize,
                        totalItems,
                        totalPages
                    }
                }",
            variables = new
            {
                wishlistId = TestingUnauthorizedWishlistId,
                pageNumber = 0,
                pageSize = 2
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var errors = (JArray?) jsonObject?.errors;

        Assert.NotNull(errors);
        Assert.True(errors.Count > 0);
    }

    [Fact]
    public async Task DeletePersonalWishlist_NotExistingWishlistId_ReturnsError()
    {
        await LoginAsync(TestingUserEmail, TestingUserPassword);
        var mutation = new
        {
            query = @"
                mutation deletePersonalWishlist($wishlistId: String!) {
                    deletePersonalWishlist (wishlistId: $wishlistId) { 
                        createdById, id, name, type
                    }
                }",
            variables = new
            {
                wishlistId = TestingNotExistingWishlistId
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var errors = (JArray?) jsonObject?.errors;

        Assert.NotNull(errors);
        Assert.True(errors.Count > 0);
    }

    [Fact]
    public async Task DeletePersonalWishlist_OtherUserWishlistId_ReturnsError()
    {
        await LoginAsync(TestingUserEmail, TestingUserPassword);
        var mutation = new
        {
            query = @"
                mutation deletePersonalWishlist($wishlistId: String!) {
                    deletePersonalWishlist (wishlistId: $wishlistId) {
                        createdById, id, name, type
                    }
                }",
            variables = new
            {
                wishlistId = TestingUnauthorizedWishlistId
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(mutation);
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
                wishlistId = TestingNotExistingWishlistId,
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
