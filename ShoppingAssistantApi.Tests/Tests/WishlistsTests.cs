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

    private const string TESTING_WISHLIST_ID = "ab79cde6f69abcd3efab65cd";

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
                pageSize = 1
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
        Console.WriteLine(personalWishlistsPageItems[0].id);
        Console.WriteLine(personalWishlistsPageItems[0].name);
        Console.WriteLine(personalWishlistsPageItems[0].type);

        Assert.NotEmpty(personalWishlistsPageItems);
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
                wishlistId = TESTING_WISHLIST_ID,
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
    public async Task AddMessageToPersonalWishlist_InvalidMessageModel_ReturnsInternalServerError()
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
                wishlistId = "8125jad7g12", // Invalid wishlistId
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
}
