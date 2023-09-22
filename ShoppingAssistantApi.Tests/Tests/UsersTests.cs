using ShoppingAssistantApi.Tests.TestExtentions;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using Xunit;
using Newtonsoft.Json;
using ShoppingAssistantApi.Application.Models.Dtos;

namespace ShoppingAssistantApi.Tests.Tests;

[Collection("Tests")]
public class UsersTests : IClassFixture<TestingFactory<Program>>
{
    private readonly HttpClient _httpClient;

    public UsersTests(TestingFactory<Program> factory)
    {
        _httpClient = factory.CreateClient();
        factory.InitialaizeData().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task UpdateUserAsync_ValidUserModel_ReturnsUpdateUserModel()
    {
        var tokensModel = await AccessExtention.CreateGuest(Guid.NewGuid().ToString(), _httpClient);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokensModel.AccessToken);

        var user = await UserExtention.GetCurrentUser(_httpClient);

        var roles = new object[1];

        foreach(var role in user.Roles)
        {
            roles[0] = new
            {
                id = role.Id,
                name = role.Name
            };
        }

        var mutation = new
        {
            query = "mutation UpdateUser($userDto: UserDtoInput!) { updateUser(userDto: $userDto) { tokens { accessToken, refreshToken }, user { email } }}",
            variables = new
            {
                userDto = new
                {
                    id = user.Id,
                    guestId = user.GuestId,
                    roles = roles,
                    email = "testing@gmail.com",
                    password = "Yuiop12345",
                    refreshTokenExpiryDate = user.RefreshTokenExpiryDate
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

        var accessTokenResult = (string)document.data.updateUser.tokens.accessToken;
        var refreshTokenResult = (string)document.data.updateUser.tokens.refreshToken;
        var userResult = JsonConvert.DeserializeObject<UserDto>(document.data.updateUser.user.ToString());

        Assert.NotNull(accessTokenResult);
        Assert.NotNull(refreshTokenResult);
        Assert.NotNull(userResult.Email);
    }

    [Fact]
    public async Task UpdateUserByAdminAsync_ValidUserModel_ReturnsUpdateUserModel()
    {
        var tokensModel = await AccessExtention.CreateGuest(new Guid().ToString(), _httpClient);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokensModel.AccessToken);

        var user = await UserExtention.GetCurrentUser(_httpClient);

        var roles = new object[1];

        foreach (var role in user.Roles)
        {
            roles[0] = new
            {
                id = role.Id,
                name = role.Name,
            };
        }

        var mutation = new
        {
            query = "mutation UpdateUserByAdmin($id: String!, $userDto: UserDtoInput!) { updateUserByAdmin(id: $id, userDto: $userDto) { tokens { accessToken, refreshToken }, user { guestId } }}",
            variables = new
            {
                id = user.Id,
                userDto = new
                {
                    id = user.Id,
                    guestId = Guid.NewGuid().ToString(),
                    roles = roles,
                    refreshTokenExpiryDate = user.RefreshTokenExpiryDate
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

        var accessTokenResult = (string)document.data.updateUserByAdmin.tokens.accessToken;
        var refreshToken = (string)document.data.updateUserByAdmin.tokens.refreshToken;
        var updatedUserGuestId = (Guid)document.data.updateUserByAdmin.user.guestId;

        Assert.NotNull(accessTokenResult);
        Assert.NotNull(refreshToken);
        Assert.NotEqual(user.GuestId, updatedUserGuestId);
    }

    [Fact]
    public async Task GetUserAsync_ValidUserId_ReturnsUser()
    {
        var tokensModel = await AccessExtention.Login("mykhailo.bilodid@nure.ua", "Yuiop12345", _httpClient);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokensModel.AccessToken);

        var usersPage = await UserExtention.GetUsers(10, _httpClient);
        var query = new
        {
            query = "query User($id: String!) { user(id: $id) { id, email, phone }}",
            variables = new
            {
                id = usersPage[0].Id,
            }
        };

        var jsonPayload = JsonConvert.SerializeObject(query);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseString = await response.Content.ReadAsStringAsync();
        var document = JsonConvert.DeserializeObject<dynamic>(responseString);
        var userResult = JsonConvert.DeserializeObject<UserDto>(document.data.user.ToString());
        Assert.Equal(userResult.Id, usersPage[0].Id);
    }

    [Fact]
    public async Task GetUserAsync_InvalidUserId_ReturnsInternalServerError()
    {
        var tokensModel = await AccessExtention.Login("mykhailo.bilodid@nure.ua", "Yuiop12345", _httpClient);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokensModel.AccessToken);

        var query = new
        {
            query = "query User($id: String!) { user(id: $id) { id, email, phone }}",
            variables = new
            {
                id = "error",
            }
        };

        var jsonPayload = JsonConvert.SerializeObject(query);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task GetCurrentUserAsync_ValidCredentials_ReturnsCurrentUser()
    {
        var tokensModel = await AccessExtention.Login("mykhailo.bilodid@nure.ua", "Yuiop12345", _httpClient);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokensModel.AccessToken);

        var query = new
        {
            query = "query CurrentUser { currentUser { id, email, phone }}",
            variables = new { }
        };

        var jsonPayload = JsonConvert.SerializeObject(query);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseString = await response.Content.ReadAsStringAsync();
        var document = JsonConvert.DeserializeObject<dynamic>(responseString);

        var user = JsonConvert.DeserializeObject<UserDto>(document.data.currentUser.ToString());
        Assert.NotEmpty(user.Id);
        Assert.NotEmpty(user.Email);
        Assert.NotEmpty(user.Phone);
        Assert.Equal(user.Email, "mykhailo.bilodid@nure.ua");
    }

    [Fact]
    public async Task GetUsersPageAsync_ValidPageNumberAndSize_ReturnsUsersPage()
    {
        var tokensModel = await AccessExtention.Login("mykhailo.bilodid@nure.ua", "Yuiop12345", _httpClient);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokensModel.AccessToken);

        var query = new
        {
            query = "query UsersPage($pageNumber: Int!, $pageSize: Int!) { usersPage(pageNumber: $pageNumber, pageSize: $pageSize) { items { id, email, phone }}}",
            variables = new
            {
                pageNumber = 1,
                pageSize = 10
            }
        };

        var jsonPayload = JsonConvert.SerializeObject(query);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseString = await response.Content.ReadAsStringAsync();
        var document = JsonConvert.DeserializeObject<dynamic>(responseString);

        var items = document.data.usersPage.items;
        Assert.NotEmpty(items);
    }
}
