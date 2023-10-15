using ShoppingAssistantApi.Tests.TestExtentions;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using Xunit;
using Newtonsoft.Json;
using ShoppingAssistantApi.Domain.Enums;

namespace ShoppingAssistantApi.Tests.Tests;

[Collection("Tests")]
public class WishlistsTests : IClassFixture<TestingFactory<Program>>
{
    private readonly HttpClient _httpClient;

    private const string WISHLIST_TESTING_USER_EMAIL = "shopping.assistant.team@gmail.com";

    private const string WISHLIST_TESTING_USER_PASSWORD = "Yuiop12345";

    private const string WISHLIST_TESTING_VALID_WISHLIST_ID = "ab79cde6f69abcd3efab65cd";

    private const string WISHLIST_TESTING_VALID_WISHLIST_NAME = "Gaming PC";

    private const WishlistTypes WISHLIST_TESTING_VALID_WISHLIST_TYPE = WishlistTypes.Product;

    private const string WISHLIST_TESTING_INVALID_WISHLIST_ID = "1234567890abcdef12345678";

    private const string WISHLIST_TESTING_OTHER_USER_WISHLIST_ID = "ab6c2c2d9edf39abcd1ef9ab";

    public WishlistsTests(TestingFactory<Program> factory)
    {
        _httpClient = factory.CreateClient();
        factory.InitialaizeData().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task StartPersonalWishlistAsync_ValidWishlistModel_ReturnsNewWishlistModels()
    {
        var tokensModel = await AccessExtention.Login(WISHLIST_TESTING_USER_EMAIL, WISHLIST_TESTING_USER_PASSWORD, _httpClient);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokensModel.AccessToken);
        var user = await UserExtention.GetCurrentUser(_httpClient);

        var mutation = new
        {
            query = "mutation startPersonalWishlist($dto: WishlistCreateDtoInput!) { startPersonalWishlist (dto: $dto) { id, name, type, createdById } }",
            variables = new
            {
                dto = new
                {
                    firstMessageText = "First message",
                    type = WishlistTypes.Product.ToString()
                }
            }
        };

        var jsonPayload = JsonConvert.SerializeObject(mutation);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseString = await response.Content.ReadAsStringAsync();
        var document = JsonConvert.DeserializeObject<dynamic>(responseString);

        var wishlistId = (string) document.data.startPersonalWishlist.id;
        var wishlistCreatedById = (string) document.data.startPersonalWishlist.createdById;
        var wishlistType = (string) document.data.startPersonalWishlist.type;
        var wishlistName = (string) document.data.startPersonalWishlist.name;

        Assert.Equal(user.Id, wishlistCreatedById);
        Assert.Equal(WishlistTypes.Product.ToString(), wishlistType);
        Assert.Equal($"{WishlistTypes.Product} Search", wishlistName);
    }

    [Fact]
    public async Task GetPersonalWishlistsPage_ValidPageNumberAndSize_ReturnsPage()
    {
        var tokensModel = await AccessExtention.Login(WISHLIST_TESTING_USER_EMAIL, WISHLIST_TESTING_USER_PASSWORD, _httpClient);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokensModel.AccessToken);
        var user = await UserExtention.GetCurrentUser(_httpClient);

        var query = new
        {
            query = "query personalWishlistsPage($pageNumber: Int!, $pageSize: Int!) { personalWishlistsPage(pageNumber: $pageNumber, pageSize: $pageSize) { items { createdById, id, name, type } } }",
            variables = new
            {
                pageNumber = 1,
                pageSize = 5
            }
        };

        var jsonPayload = JsonConvert.SerializeObject(query);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseString = await response.Content.ReadAsStringAsync();
        var document = JsonConvert.DeserializeObject<dynamic>(responseString);

        var personalWishlistsPageItems = Enumerable.ToList(document.data.personalWishlistsPage.items);
        var personalWishlistCreatedById = (string) personalWishlistsPageItems[0].createdById;

        Assert.NotEmpty(personalWishlistsPageItems);
        Assert.Equal(user.Id, personalWishlistCreatedById);
    }

    [Fact]
    public async Task GetPersonalWishlist_ValidWishlistIdOrAuthorizedAccess_ReturnsWishlistDto()
    {
        var tokensModel = await AccessExtention.Login(WISHLIST_TESTING_USER_EMAIL, WISHLIST_TESTING_USER_PASSWORD, _httpClient);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokensModel.AccessToken);
        var user = await UserExtention.GetCurrentUser(_httpClient);

        var query = new
        {
            query = "query personalWishlist($wishlistId: String!) { personalWishlist(wishlistId: $wishlistId) { createdById, id, name, type } }",
            variables = new
            {
                wishlistId = WISHLIST_TESTING_VALID_WISHLIST_ID
            }
        };

        var jsonPayload = JsonConvert.SerializeObject(query);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseString = await response.Content.ReadAsStringAsync();
        var document = JsonConvert.DeserializeObject<dynamic>(responseString);

        var personalWishlistId = (string) document.data.personalWishlist.id;
        var personalWishlistName = (string) document.data.personalWishlist.name;
        var personalWishlistType = (string) document.data.personalWishlist.type;
        var personalWishlistCreatedById = (string) document.data.personalWishlist.createdById;

        Assert.Equal(WISHLIST_TESTING_VALID_WISHLIST_ID, personalWishlistId);
        Assert.Equal(WISHLIST_TESTING_VALID_WISHLIST_NAME, personalWishlistName);
        Assert.Equal(WISHLIST_TESTING_VALID_WISHLIST_TYPE.ToString(), personalWishlistType);
        Assert.Equal(user.Id, personalWishlistCreatedById);
    }

    [Fact]
    public async Task AddMessageToPersonalWishlist_ValidMessageModel_ReturnsNewMessageModel()
    {
        var tokensModel = await AccessExtention.Login(WISHLIST_TESTING_USER_EMAIL, WISHLIST_TESTING_USER_PASSWORD, _httpClient);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokensModel.AccessToken);
        var user = await UserExtention.GetCurrentUser(_httpClient);

        const string MESSAGE_TEXT = "Second Message";

        var mutation = new
        {
            query = "mutation addMessageToPersonalWishlist($wishlistId: String!, $dto: MessageCreateDtoInput!) { addMessageToPersonalWishlist (wishlistId: $wishlistId, dto: $dto) { role, text, createdById } }",
            variables = new
            {
                wishlistId = WISHLIST_TESTING_VALID_WISHLIST_ID,
                dto = new
                {
                    text = MESSAGE_TEXT
                }
            }
        };

        var jsonPayload = JsonConvert.SerializeObject(mutation);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseString = await response.Content.ReadAsStringAsync();
        var document = JsonConvert.DeserializeObject<dynamic>(responseString);

        var messageRole = (string) document.data.addMessageToPersonalWishlist.role;
        var messageText = (string) document.data.addMessageToPersonalWishlist.text;
        var messageCreatedById = (string) document.data.addMessageToPersonalWishlist.createdById;

        Assert.Equal(MessageRoles.User.ToString(), messageRole);
        Assert.Equal(MESSAGE_TEXT, messageText);
        Assert.Equal(user.Id, messageCreatedById);
    }

    [Fact]
    public async Task GetMessagesPageFromPersonalWishlist_ValidPageNumberAndSizeValidWishlistIdOrAuthorizedAccess_ReturnsPage()
    {
        var tokensModel = await AccessExtention.Login(WISHLIST_TESTING_USER_EMAIL, WISHLIST_TESTING_USER_PASSWORD, _httpClient);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokensModel.AccessToken);
        var user = await UserExtention.GetCurrentUser(_httpClient);

        var mutation = new
        {
            query = "query messagesPageFromPersonalWishlist($wishlistId: String!, $pageNumber: Int!, $pageSize: Int!) { messagesPageFromPersonalWishlist (wishlistId: $wishlistId, pageNumber: $pageNumber, pageSize: $pageSize) { hasNextPage, hasPreviousPage, items { id, text, role, createdById }, pageNumber, pageSize, totalItems, totalPages } }",
            variables = new
            {
                wishlistId = WISHLIST_TESTING_VALID_WISHLIST_ID,
                pageNumber = 1,
                pageSize = 2
            }
        };

        var jsonPayload = JsonConvert.SerializeObject(mutation);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseString = await response.Content.ReadAsStringAsync();
        var document = JsonConvert.DeserializeObject<dynamic>(responseString);

        var messagesPageFromPersonalWishlist = Enumerable.ToList(document.data.messagesPageFromPersonalWishlist.items);
        var firstMessageInPage = messagesPageFromPersonalWishlist[0];
        var secondMessageInPage = messagesPageFromPersonalWishlist[1];

        Assert.Equal("Message 5", (string) firstMessageInPage.text);
        Assert.Equal(MessageRoles.User.ToString(), (string) firstMessageInPage.role);
        Assert.Equal(user.Id, (string) firstMessageInPage.createdById);
    }

    [Fact]
    public async Task AddProductToPersonalWishlist_ValidMessageModel_ReturnsNewProductModel()
    {
        var tokensModel = await AccessExtention.Login(WISHLIST_TESTING_USER_EMAIL, WISHLIST_TESTING_USER_PASSWORD, _httpClient);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokensModel.AccessToken);
        var user = await UserExtention.GetCurrentUser(_httpClient);

        var mutation = new
        {
            query = "mutation addProductToPersonalWishlist($wishlistId: String!, $dto: ProductCreateDtoInput!) { addProductToPersonalWishlist (wishlistId: $wishlistId, dto: $dto) { url, name, description, rating, imagesUrls, wasOpened } }",
            variables = new
            {
                wishlistId = WISHLIST_TESTING_VALID_WISHLIST_ID,
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

        var jsonPayload = JsonConvert.SerializeObject(mutation);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseString = await response.Content.ReadAsStringAsync();
        var document = JsonConvert.DeserializeObject<dynamic>(responseString);

        Assert.Equal("https://www.amazon.com/url", (string) document.data.addProductToPersonalWishlist.url);
        Assert.Equal("Generic name", (string) document.data.addProductToPersonalWishlist.name);
        Assert.Equal("Generic description", (string) document.data.addProductToPersonalWishlist.description);
        Assert.Equal(4.8, (double) document.data.addProductToPersonalWishlist.rating);
        Assert.Equal("https://www.amazon.com/image-url-1", (string) document.data.addProductToPersonalWishlist.imagesUrls[0]);
    }

    [Fact]
    public async Task GetProductsPageFromPersonalWishlist_ValidPageNumberAndSizeValidWishlistIdOrAuthorizedAccess_ReturnsPage()
    {
        var tokensModel = await AccessExtention.Login(WISHLIST_TESTING_USER_EMAIL, WISHLIST_TESTING_USER_PASSWORD, _httpClient);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokensModel.AccessToken);
        var user = await UserExtention.GetCurrentUser(_httpClient);

        var mutation = new
        {
            query = "query productsPageFromPersonalWishlist($wishlistId: String!, $pageNumber: Int!, $pageSize: Int!) { productsPageFromPersonalWishlist (wishlistId: $wishlistId, pageNumber: $pageNumber, pageSize: $pageSize) { hasNextPage, hasPreviousPage, items { id, url, name, description, rating, imagesUrls, wasOpened, wishlistId }, pageNumber, pageSize, totalItems, totalPages } }",
            variables = new
            {
                wishlistId = WISHLIST_TESTING_VALID_WISHLIST_ID,
                pageNumber = 1,
                pageSize = 2
            }
        };

        var jsonPayload = JsonConvert.SerializeObject(mutation);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseString = await response.Content.ReadAsStringAsync();
        var document = JsonConvert.DeserializeObject<dynamic>(responseString);

        var productsPageFromPersonalWishlist = Enumerable.ToList(document.data.productsPageFromPersonalWishlist.items);
        var secondProductInPage = productsPageFromPersonalWishlist[1];

        Assert.Equal("Samsung 970 EVO Plus SSD 2TB NVMe M.2 Internal Solid State Hard Drive, V-NAND Technology, Storage and Memory Expansion for Gaming, Graphics w/ Heat Control, Max Speed, MZ-V7S2T0B/AM ", (string) secondProductInPage.name);
        Assert.Equal(WISHLIST_TESTING_VALID_WISHLIST_ID, (string) secondProductInPage.wishlistId);
    }

    [Fact]
    public async Task DeletePersonalWishlist_ValidWishlistIdOrAuthorizedAccess_ReturnsWishlistModel()
    {
        var tokensModel = await AccessExtention.Login(WISHLIST_TESTING_USER_EMAIL, WISHLIST_TESTING_USER_PASSWORD, _httpClient);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokensModel.AccessToken);
        var user = await UserExtention.GetCurrentUser(_httpClient);

        var mutation = new
        {
            query = "mutation deletePersonalWishlist($wishlistId: String!) { deletePersonalWishlist (wishlistId: $wishlistId) { createdById, id, name, type } }",
            variables = new
            {
                wishlistId = WISHLIST_TESTING_VALID_WISHLIST_ID
            }
        };

        var jsonPayload = JsonConvert.SerializeObject(mutation);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseString = await response.Content.ReadAsStringAsync();
        var document = JsonConvert.DeserializeObject<dynamic>(responseString);

        var personalWishlistId = (string) document.data.deletePersonalWishlist.id;
        var personalWishlistName = (string) document.data.deletePersonalWishlist.name;
        var personalWishlistType = (string) document.data.deletePersonalWishlist.type;
        var personalWishlistCreatedById = (string) document.data.deletePersonalWishlist.createdById;

        Assert.Equal(WISHLIST_TESTING_VALID_WISHLIST_ID, personalWishlistId);
        Assert.Equal(WISHLIST_TESTING_VALID_WISHLIST_NAME, personalWishlistName);
        Assert.Equal(WISHLIST_TESTING_VALID_WISHLIST_TYPE.ToString(), personalWishlistType);
        Assert.Equal(user.Id, personalWishlistCreatedById);
    }

    [Fact]
    public async Task StartPersonalWishlistAsync_InvalidWishlistModel_ReturnsInternalServerError()
    {
        var tokensModel = await AccessExtention.Login(WISHLIST_TESTING_USER_EMAIL, WISHLIST_TESTING_USER_PASSWORD, _httpClient);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokensModel.AccessToken);
        var user = await UserExtention.GetCurrentUser(_httpClient);

        var mutation = new
        {
            query = "mutation startPersonalWishlist($dto: WishlistCreateDtoInput!) { startPersonalWishlist (dto: $dto) { id, name, type, createdById } }",
            variables = new
            {
                dto = new
                {
                    firstMessageText = "First message",
                    type = "Invalid type" // Invalid Wishlist Type
                }
            }
        };

        var jsonPayload = JsonConvert.SerializeObject(mutation);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task GetPersonalWishlistsPage_InValidPageNumber_ReturnsEmptyList()
    {
        var tokensModel = await AccessExtention.Login(WISHLIST_TESTING_USER_EMAIL, WISHLIST_TESTING_USER_PASSWORD, _httpClient);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokensModel.AccessToken);
        var user = await UserExtention.GetCurrentUser(_httpClient);

        var query = new
        {
            query = "query personalWishlistsPage($pageNumber: Int!, $pageSize: Int!) { personalWishlistsPage(pageNumber: $pageNumber, pageSize: $pageSize) { items { createdById, id, name, type } } }",
            variables = new
            {
                pageNumber = 100,
                pageSize = 1
            }
        };

        var jsonPayload = JsonConvert.SerializeObject(query);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseString = await response.Content.ReadAsStringAsync();
        var document = JsonConvert.DeserializeObject<dynamic>(responseString);

        Console.WriteLine(document);

        var personalWishlistsPageItems = Enumerable.ToList(document.data.personalWishlistsPage.items);

        Assert.Empty(personalWishlistsPageItems);
    }

    [Fact]
    public async Task GetPersonalWishlistsPage_InValidPageSize_ReturnsPage()
    {
        var tokensModel = await AccessExtention.Login(WISHLIST_TESTING_USER_EMAIL, WISHLIST_TESTING_USER_PASSWORD, _httpClient);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokensModel.AccessToken);
        var user = await UserExtention.GetCurrentUser(_httpClient);

        var query = new
        {
            query = "query personalWishlistsPage($pageNumber: Int!, $pageSize: Int!) { personalWishlistsPage(pageNumber: $pageNumber, pageSize: $pageSize) { items { createdById, id, name, type } } }",
            variables = new
            {
                pageNumber = 1,
                pageSize = 100
            }
        };

        var jsonPayload = JsonConvert.SerializeObject(query);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseString = await response.Content.ReadAsStringAsync();
        var document = JsonConvert.DeserializeObject<dynamic>(responseString);

        Console.WriteLine(document);


        var personalWishlistsPageItems = Enumerable.ToList(document.data.personalWishlistsPage.items);
        var personalWishlistCreatedById = (string) personalWishlistsPageItems[0].createdById;

        Assert.NotEmpty(personalWishlistsPageItems);
        Assert.Equal(user.Id, personalWishlistCreatedById);
    }

    [Fact]
    public async Task GetPersonalWishlist_InvalidWishlistId_ReturnsInternalServerError()
    {
        var tokensModel = await AccessExtention.Login(WISHLIST_TESTING_USER_EMAIL, WISHLIST_TESTING_USER_PASSWORD, _httpClient);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokensModel.AccessToken);
        var user = await UserExtention.GetCurrentUser(_httpClient);

        var query = new
        {
            query = "query personalWishlist($wishlistId: String!) { personalWishlist(wishlistId: $wishlistId) { createdById, id, name, type } }",
            variables = new
            {
                wishlistId = WISHLIST_TESTING_INVALID_WISHLIST_ID
            }
        };

        var jsonPayload = JsonConvert.SerializeObject(query);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task GetPersonalWishlist_UnAuthorizedAccess_ReturnsInternalServerError()
    {
        var tokensModel = await AccessExtention.Login(WISHLIST_TESTING_USER_EMAIL, WISHLIST_TESTING_USER_PASSWORD, _httpClient);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokensModel.AccessToken);
        var user = await UserExtention.GetCurrentUser(_httpClient);

        var query = new
        {
            query = "query personalWishlist($wishlistId: String!) { personalWishlist(wishlistId: $wishlistId) { createdById, id, name, type } }",
            variables = new
            {
                wishlistId = WISHLIST_TESTING_OTHER_USER_WISHLIST_ID
            }
        };

        var jsonPayload = JsonConvert.SerializeObject(query);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task AddMessageToPersonalWishlist_InvalidWishlistId_ReturnsInternalServerError()
    {
        var tokensModel = await AccessExtention.Login(WISHLIST_TESTING_USER_EMAIL, WISHLIST_TESTING_USER_PASSWORD, _httpClient);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokensModel.AccessToken);
        var user = await UserExtention.GetCurrentUser(_httpClient);

        const string MESSAGE_TEXT = "Second Message";

        var mutation = new
        {
            query = "mutation addMessageToPersonalWishlist($wishlistId: String!, $dto: MessageCreateDtoInput!) { addMessageToPersonalWishlist (wishlistId: $wishlistId, dto: $dto) { role, text, createdById } }",
            variables = new
            {
                wishlistId = WISHLIST_TESTING_INVALID_WISHLIST_ID, 
                dto = new
                {
                    text = MESSAGE_TEXT,
                }
            }
        };

        var jsonPayload = JsonConvert.SerializeObject(mutation);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task AddMessageToPersonalWishlist_UnAuthorizedAccess_ReturnsInternalServerError()
    {
        var tokensModel = await AccessExtention.Login(WISHLIST_TESTING_USER_EMAIL, WISHLIST_TESTING_USER_PASSWORD, _httpClient);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokensModel.AccessToken);
        var user = await UserExtention.GetCurrentUser(_httpClient);

        const string MESSAGE_TEXT = "Second Message";

        var mutation = new
        {
            query = "mutation addMessageToPersonalWishlist($wishlistId: String!, $dto: MessageCreateDtoInput!) { addMessageToPersonalWishlist (wishlistId: $wishlistId, dto: $dto) { role, text, createdById } }",
            variables = new
            {
                wishlistId = WISHLIST_TESTING_OTHER_USER_WISHLIST_ID, 
                dto = new
                {
                    text = MESSAGE_TEXT,
                }
            }
        };

        var jsonPayload = JsonConvert.SerializeObject(mutation);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task GetMessagesPageFromPersonalWishlist_InValidPageNumber_ReturnsEmptyList()
    {
        var tokensModel = await AccessExtention.Login(WISHLIST_TESTING_USER_EMAIL, WISHLIST_TESTING_USER_PASSWORD, _httpClient);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokensModel.AccessToken);
        var user = await UserExtention.GetCurrentUser(_httpClient);

        var mutation = new
        {
            query = "query messagesPageFromPersonalWishlist($wishlistId: String!, $pageNumber: Int!, $pageSize: Int!) { messagesPageFromPersonalWishlist (wishlistId: $wishlistId, pageNumber: $pageNumber, pageSize: $pageSize) { hasNextPage, hasPreviousPage, items { id, text, role, createdById }, pageNumber, pageSize, totalItems, totalPages } }",
            variables = new
            {
                wishlistId = WISHLIST_TESTING_VALID_WISHLIST_ID,
                pageNumber = 4,
                pageSize = 2
            }
        };

        var jsonPayload = JsonConvert.SerializeObject(mutation);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseString = await response.Content.ReadAsStringAsync();
        var document = JsonConvert.DeserializeObject<dynamic>(responseString);

        Console.WriteLine(document);


        var messagesPageFromPersonalWishlistItems = Enumerable.ToList(document.data.messagesPageFromPersonalWishlist.items);

        Assert.Empty(messagesPageFromPersonalWishlistItems);
    }

    [Fact]
    public async Task GetMessagesPageFromPersonalWishlist_InValidPageSize_ReturnsPage()
    {
        var tokensModel = await AccessExtention.Login(WISHLIST_TESTING_USER_EMAIL, WISHLIST_TESTING_USER_PASSWORD, _httpClient);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokensModel.AccessToken);
        var user = await UserExtention.GetCurrentUser(_httpClient);

        var mutation = new
        {
            query = "query messagesPageFromPersonalWishlist($wishlistId: String!, $pageNumber: Int!, $pageSize: Int!) { messagesPageFromPersonalWishlist (wishlistId: $wishlistId, pageNumber: $pageNumber, pageSize: $pageSize) { hasNextPage, hasPreviousPage, items { id, text, role, createdById }, pageNumber, pageSize, totalItems, totalPages } }",
            variables = new
            {
                wishlistId = WISHLIST_TESTING_VALID_WISHLIST_ID,
                pageNumber = 1,
                pageSize = 10
            }
        };

        var jsonPayload = JsonConvert.SerializeObject(mutation);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseString = await response.Content.ReadAsStringAsync();
        var document = JsonConvert.DeserializeObject<dynamic>(responseString);

        Console.WriteLine(document);


        var messagesPageFromPersonalWishlist = Enumerable.ToList(document.data.messagesPageFromPersonalWishlist.items);
        var firstMessageInPage = messagesPageFromPersonalWishlist[0];
        var secondMessageInPage = messagesPageFromPersonalWishlist[1];

        Assert.Equal("Message 1", (string) firstMessageInPage.text);
        Assert.Equal(MessageRoles.User.ToString(), (string) firstMessageInPage.role);
        Assert.Equal(user.Id, (string) firstMessageInPage.createdById);
    }

    [Fact]
    public async Task GetMessagesPageFromPersonalWishlist_InValidWishlistId_ReturnsInternalServerError()
    {
        var tokensModel = await AccessExtention.Login(WISHLIST_TESTING_USER_EMAIL, WISHLIST_TESTING_USER_PASSWORD, _httpClient);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokensModel.AccessToken);
        var user = await UserExtention.GetCurrentUser(_httpClient);

        var mutation = new
        {
            query = "query messagesPageFromPersonalWishlist($wishlistId: String!, $pageNumber: Int!, $pageSize: Int!) { messagesPageFromPersonalWishlist (wishlistId: $wishlistId, pageNumber: $pageNumber, pageSize: $pageSize) { hasNextPage, hasPreviousPage, items { id, text, role, createdById }, pageNumber, pageSize, totalItems, totalPages } }",
            variables = new
            {
                wishlistId = WISHLIST_TESTING_INVALID_WISHLIST_ID,
                pageNumber = 1,
                pageSize = 2
            }
        };

        var jsonPayload = JsonConvert.SerializeObject(mutation);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task GetMessagesPageFromPersonalWishlist_UnAuthorizedAccess_ReturnsInternalServerError()
    {
        var tokensModel = await AccessExtention.Login(WISHLIST_TESTING_USER_EMAIL, WISHLIST_TESTING_USER_PASSWORD, _httpClient);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokensModel.AccessToken);
        var user = await UserExtention.GetCurrentUser(_httpClient);

        var mutation = new
        {
            query = "query messagesPageFromPersonalWishlist($wishlistId: String!, $pageNumber: Int!, $pageSize: Int!) { messagesPageFromPersonalWishlist (wishlistId: $wishlistId, pageNumber: $pageNumber, pageSize: $pageSize) { hasNextPage, hasPreviousPage, items { id, text, role, createdById }, pageNumber, pageSize, totalItems, totalPages } }",
            variables = new
            {
                wishlistId = WISHLIST_TESTING_OTHER_USER_WISHLIST_ID,
                pageNumber = 1,
                pageSize = 2
            }
        };

        var jsonPayload = JsonConvert.SerializeObject(mutation);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task AddProductToPersonalWishlist_InValidWishlistId_RturnsInternalServerError()
    {
        var tokensModel = await AccessExtention.Login(WISHLIST_TESTING_USER_EMAIL, WISHLIST_TESTING_USER_PASSWORD, _httpClient);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokensModel.AccessToken);
        var user = await UserExtention.GetCurrentUser(_httpClient);

        var mutation = new
        {
            query = "mutation addProductToPersonalWishlist($wishlistId: String!, $dto: ProductCreateDtoInput!) { addProductToPersonalWishlist (wishlistId: $wishlistId, dto: $dto) { url, name, description, rating, imagesUrls, wasOpened } }",
            variables = new
            {
                wishlistId = WISHLIST_TESTING_INVALID_WISHLIST_ID,
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

        var jsonPayload = JsonConvert.SerializeObject(mutation);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task AddProductToPersonalWishlist_UnAuthorizedAccess_RturnsInternalServerError()
    {
        var tokensModel = await AccessExtention.Login(WISHLIST_TESTING_USER_EMAIL, WISHLIST_TESTING_USER_PASSWORD, _httpClient);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokensModel.AccessToken);
        var user = await UserExtention.GetCurrentUser(_httpClient);

        var mutation = new
        {
            query = "mutation addProductToPersonalWishlist($wishlistId: String!, $dto: ProductCreateDtoInput!) { addProductToPersonalWishlist (wishlistId: $wishlistId, dto: $dto) { url, name, description, rating, imagesUrls, wasOpened } }",
            variables = new
            {
                wishlistId = WISHLIST_TESTING_OTHER_USER_WISHLIST_ID,
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

        var jsonPayload = JsonConvert.SerializeObject(mutation);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task GetProductsPageFromPersonalWishlist_InValidPageNumber_ReturnsInternalServerError()
    {
        var tokensModel = await AccessExtention.Login(WISHLIST_TESTING_USER_EMAIL, WISHLIST_TESTING_USER_PASSWORD, _httpClient);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokensModel.AccessToken);
        var user = await UserExtention.GetCurrentUser(_httpClient);

        var mutation = new
        {
            query = "query productsPageFromPersonalWishlist($wishlistId: String!, $pageNumber: Int!, $pageSize: Int!) { productsPageFromPersonalWishlist (wishlistId: $wishlistId, pageNumber: $pageNumber, pageSize: $pageSize) { hasNextPage, hasPreviousPage, items { id, url, name, description, rating, imagesUrls, wasOpened, wishlistId }, pageNumber, pageSize, totalItems, totalPages } }",
            variables = new
            {
                wishlistId = WISHLIST_TESTING_VALID_WISHLIST_ID,
                pageNumber = 0,
                pageSize = 2
            }
        };

        var jsonPayload = JsonConvert.SerializeObject(mutation);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task GetProductsPageFromPersonalWishlist_InValidPageSize_ReturnsPage()
    {
        var tokensModel = await AccessExtention.Login(WISHLIST_TESTING_USER_EMAIL, WISHLIST_TESTING_USER_PASSWORD, _httpClient);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokensModel.AccessToken);
        var user = await UserExtention.GetCurrentUser(_httpClient);

        var mutation = new
        {
            query = "query productsPageFromPersonalWishlist($wishlistId: String!, $pageNumber: Int!, $pageSize: Int!) { productsPageFromPersonalWishlist (wishlistId: $wishlistId, pageNumber: $pageNumber, pageSize: $pageSize) { hasNextPage, hasPreviousPage, items { id, url, name, description, rating, imagesUrls, wasOpened, wishlistId }, pageNumber, pageSize, totalItems, totalPages } }",
            variables = new
            {
                wishlistId = WISHLIST_TESTING_VALID_WISHLIST_ID,
                pageNumber = 1,
                pageSize = 100
            }
        };

        var jsonPayload = JsonConvert.SerializeObject(mutation);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseString = await response.Content.ReadAsStringAsync();
        var document = JsonConvert.DeserializeObject<dynamic>(responseString);

        var productsPageFromPersonalWishlist = Enumerable.ToList(document.data.productsPageFromPersonalWishlist.items);
        var secondProductInPage = productsPageFromPersonalWishlist[1];

        Assert.Equal("Samsung 970 EVO Plus SSD 2TB NVMe M.2 Internal Solid State Hard Drive, V-NAND Technology, Storage and Memory Expansion for Gaming, Graphics w/ Heat Control, Max Speed, MZ-V7S2T0B/AM ", (string) secondProductInPage.name);
        Assert.Equal(WISHLIST_TESTING_VALID_WISHLIST_ID, (string) secondProductInPage.wishlistId);
    }

    [Fact]
    public async Task GetProductsPageFromPersonalWishlist_InValidWishlistId_ReturnsInternalServerError()
    {
        var tokensModel = await AccessExtention.Login(WISHLIST_TESTING_USER_EMAIL, WISHLIST_TESTING_USER_PASSWORD, _httpClient);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokensModel.AccessToken);
        var user = await UserExtention.GetCurrentUser(_httpClient);

        var mutation = new
        {
            query = "query productsPageFromPersonalWishlist($wishlistId: String!, $pageNumber: Int!, $pageSize: Int!) { productsPageFromPersonalWishlist (wishlistId: $wishlistId, pageNumber: $pageNumber, pageSize: $pageSize) { hasNextPage, hasPreviousPage, items { id, url, name, description, rating, imagesUrls, wasOpened, wishlistId }, pageNumber, pageSize, totalItems, totalPages } }",
            variables = new
            {
                wishlistId = WISHLIST_TESTING_INVALID_WISHLIST_ID,
                pageNumber = 0,
                pageSize = 2
            }
        };

        var jsonPayload = JsonConvert.SerializeObject(mutation);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task GetProductsPageFromPersonalWishlist_UnAuthorizedAccess_ReturnsInternalServerError()
    {
        var tokensModel = await AccessExtention.Login(WISHLIST_TESTING_USER_EMAIL, WISHLIST_TESTING_USER_PASSWORD, _httpClient);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokensModel.AccessToken);
        var user = await UserExtention.GetCurrentUser(_httpClient);

        var mutation = new
        {
            query = "query productsPageFromPersonalWishlist($wishlistId: String!, $pageNumber: Int!, $pageSize: Int!) { productsPageFromPersonalWishlist (wishlistId: $wishlistId, pageNumber: $pageNumber, pageSize: $pageSize) { hasNextPage, hasPreviousPage, items { id, url, name, description, rating, imagesUrls, wasOpened, wishlistId }, pageNumber, pageSize, totalItems, totalPages } }",
            variables = new
            {
                wishlistId = WISHLIST_TESTING_OTHER_USER_WISHLIST_ID,
                pageNumber = 0,
                pageSize = 2
            }
        };

        var jsonPayload = JsonConvert.SerializeObject(mutation);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task DeletePersonalWishlist_InValidWishlistId_ReturnsInternalServerError()
    {
        var tokensModel = await AccessExtention.Login(WISHLIST_TESTING_USER_EMAIL, WISHLIST_TESTING_USER_PASSWORD, _httpClient);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokensModel.AccessToken);
        var user = await UserExtention.GetCurrentUser(_httpClient);

        var mutation = new
        {
            query = "mutation deletePersonalWishlist($wishlistId: String!) { deletePersonalWishlist (wishlistId: $wishlistId) { createdById, id, name, type } }",
            variables = new
            {
                wishlistId = WISHLIST_TESTING_INVALID_WISHLIST_ID
            }
        };

        var jsonPayload = JsonConvert.SerializeObject(mutation);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task DeletePersonalWishlist_UnAuthorizedAccess_ReturnsInternalServerError()
    {
        var tokensModel = await AccessExtention.Login(WISHLIST_TESTING_USER_EMAIL, WISHLIST_TESTING_USER_PASSWORD, _httpClient);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokensModel.AccessToken);
        var user = await UserExtention.GetCurrentUser(_httpClient);

        var mutation = new
        {
            query = "mutation deletePersonalWishlist($wishlistId: String!) { deletePersonalWishlist (wishlistId: $wishlistId) { createdById, id, name, type } }",
            variables = new
            {
                wishlistId = WISHLIST_TESTING_OTHER_USER_WISHLIST_ID
            }
        };

        var jsonPayload = JsonConvert.SerializeObject(mutation);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }
}
