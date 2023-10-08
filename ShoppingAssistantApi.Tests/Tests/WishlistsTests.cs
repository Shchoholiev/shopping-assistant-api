using ShoppingAssistantApi.Tests.TestExtentions;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using Xunit;
using Newtonsoft.Json;
using ShoppingAssistantApi.Application.Models.Dtos;
using ShoppingAssistantApi.Domain.Enums;

namespace ShoppingAssistantApi.Tests.Tests;

[Collection("Tests")]
public class WishlistsTests : IClassFixture<TestingFactory<Program>>
{
    private readonly HttpClient _httpClient;

    public WishlistsTests(TestingFactory<Program> factory)
    {
        _httpClient = factory.CreateClient();
        factory.InitialaizeData().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task StartPersonalWishlistAndAddMessageAsync_ValidWishlistAndMessageModels_ReturnsNewWishlistAndMessageModels()
    {
        var tokensModel = await AccessExtention.CreateGuest(new Guid().ToString(), _httpClient);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokensModel.AccessToken);
        var user = await UserExtention.GetCurrentUser(_httpClient);

        var startPersonalWishlistMutation = new
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

        var jsonPayload = JsonConvert.SerializeObject(startPersonalWishlistMutation);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var startPersonalWishlistResponse = await _httpClient.PostAsync("graphql", content);
        startPersonalWishlistResponse.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, startPersonalWishlistResponse.StatusCode);

        var responseString = await startPersonalWishlistResponse.Content.ReadAsStringAsync();
        var document = JsonConvert.DeserializeObject<dynamic>(responseString);

        var wishlistId = (string) document.data.startPersonalWishlist.id;
        var wishlistCreatedById = (string) document.data.startPersonalWishlist.createdById;
        var wishlistType = (string) document.data.startPersonalWishlist.type;
        var wishlistName = (string) document.data.startPersonalWishlist.name;
        
        Assert.Equal(user.Id, wishlistCreatedById);
        Assert.Equal(WishlistTypes.Product.ToString(), wishlistType);
        Assert.Equal($"{WishlistTypes.Product} Search", wishlistName);

        const string MESSAGE_TEXT = "Second Message";

        var addMessageToPersonalWishlistMutation = new
        {
            query = "mutation addMessageToPersonalWishlist($wishlistId: String!, $dto: MessageCreateDtoInput!) { addMessageToPersonalWishlist (wishlistId: $wishlistId, dto: $dto) { role, text, createdById } }",
            variables = new
            {
                wishlistId = wishlistId,
                dto = new
                {
                    text = MESSAGE_TEXT,
                }
            }
        };

        jsonPayload = JsonConvert.SerializeObject(addMessageToPersonalWishlistMutation);
        content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var addMessageToPersonalWishlistResponse = await _httpClient.PostAsync("graphql", content);
        addMessageToPersonalWishlistResponse.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, addMessageToPersonalWishlistResponse.StatusCode);

        responseString = await addMessageToPersonalWishlistResponse.Content.ReadAsStringAsync();
        document = JsonConvert.DeserializeObject<dynamic>(responseString);

        var messageRole = (string) document.data.addMessageToPersonalWishlist.role;
        var messageText = (string) document.data.addMessageToPersonalWishlist.text;
        var messageCreatedById = (string) document.data.addMessageToPersonalWishlist.createdById;
        
        Assert.Equal(MessageRoles.User.ToString(), messageRole);
        Assert.Equal(MESSAGE_TEXT, messageText);
        Assert.Equal(user.Id, messageCreatedById);
    }
}
