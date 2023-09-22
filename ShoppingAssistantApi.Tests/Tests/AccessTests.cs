using System.Net;
using System.Text;
using Xunit;
using ShoppingAssistantApi.Tests.TestExtentions;
using Newtonsoft.Json;

namespace ShoppingAssistantApi.Tests.Tests;

[Collection("Tests")]

public class AccessTests : IClassFixture<TestingFactory<Program>>
{
    private readonly HttpClient _httpClient;

    public AccessTests(TestingFactory<Program> factory)
    {
        _httpClient = factory.CreateClient();
        factory.InitialaizeData().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task AccessGuestAsync_ValidGuid_ReturnsTokensModel()
    {
        var mutation = new
        {
            query = "mutation AccessGuest($guest: AccessGuestModelInput!) { accessGuest(guest: $guest) { accessToken, refreshToken } }",
            variables = new
            {
                guest = new
                {
                    guestId = Guid.NewGuid(),
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
        
        var accessToken = (string)document.data.accessGuest.accessToken;
        var refreshToken = (string)document.data.accessGuest.refreshToken;

        Assert.NotNull(accessToken);
        Assert.NotNull(refreshToken);
        }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-guid-format")]
    public async Task AccessGuestAsync_InvalidGuid_ReturnsInternalServerError(string guestId)
    {
        var mutation = new
        {
            query = "mutation AccessGuest($guest: AccessGuestModelInput!) { accessGuest(guest: $guest) { accessToken, refreshToken } }",
            variables = new
            {
                guest = new
                {
                    guestId
                }
            }
        };

        var jsonPayload = JsonConvert.SerializeObject(mutation);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Theory]
    [InlineData("invalid-email-format", null, "Yuiop12345")]
    [InlineData(null, null, "Yuiop12345")]
    [InlineData(null, null, "")]
    [InlineData("mihail.beloded.work@gmail.com", null, "")]
    public async Task LoginAsync_InvalidCredentials_ReturnsInternalServerError(string email, string phone, string password)
    {
        var mutation = new
        {
            query = "mutation Login($login: AccessUserModelInput!) { login(login: $login) { accessToken refreshToken }}",
            variables = new
            {
                login = new
                {
                    phone = phone,
                    email = email,
                    password = password
                }
            }
        };

        var jsonPayload = JsonConvert.SerializeObject(mutation);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Theory]
    [InlineData("mykhailo.bilodid@nure.ua", "+380953326869", "Yuiop12345")]
    [InlineData(null, "+380953326888", "Yuiop12345")]
    [InlineData("mykhailo.bilodid@nure.ua", null, "Yuiop12345")]
    public async Task LoginAsync_ValidCredentials_ReturnsTokensModel(string email, string phone, string password)
    {
        var mutation = new
        {
            query = "mutation Login($login: AccessUserModelInput!) { login(login: $login) { accessToken refreshToken }}",
            variables = new
            {
                login = new
                {
                    phone = phone,
                    email = email,
                    password = password
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

        var accessToken = (string)document.data.login.accessToken;
        var refreshToken = (string)document.data.login.refreshToken;

        Assert.NotNull(accessToken);
        Assert.NotNull(refreshToken);
    }

    [Fact]
    public async Task RefreshUserTokenAsync_ValidTokensModel_ReturnsTokensModel()
    {
        var tokensModel = await AccessExtention.CreateGuest(new Guid().ToString(), _httpClient);
        var accessToken = tokensModel.AccessToken;
        var refreshToken = tokensModel.RefreshToken;

        var mutation = new
        {
            query = "mutation RefreshToken($model: TokensModelInput!) { refreshUserToken(model: $model) { accessToken refreshToken }}",
            variables = new
            {
                model = new
                {
                    accessToken,
                    refreshToken
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

        var accessTokenResult = (string)document.data.refreshUserToken.accessToken;
        var refreshTokenResult = (string)document.data.refreshUserToken.refreshToken;

        Assert.NotNull(accessTokenResult);
        Assert.NotNull(refreshTokenResult);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("invalid-access-token", "invalid-refresh-token")]
    public async Task RefreshUserTokenAsync_InvalidTokensModel_ReturnsInternalServerError(string refreshToken, string accessToken)
    {
        var mutation = new
        {
            query = "mutation RefreshToken($model: TokensModelInput!) { refreshUserToken(model: $model) { accessToken refreshToken }}",
            variables = new
            {
                model = new
                {
                    accessToken,
                    refreshToken
                }
            }
        };

        var jsonPayload = JsonConvert.SerializeObject(mutation);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }
}