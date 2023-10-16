using Newtonsoft.Json.Linq;
using ShoppingAssistantApi.Application.Models.Identity;
using ShoppingAssistantApi.Tests.TestExtentions;
using Xunit;

namespace ShoppingAssistantApi.Tests.Tests;

// TODO: make errors test more descrptive
public class AccessTests : TestsBase
{
    public AccessTests(TestingFactory<Program> factory)
        : base(factory)
    { }

    [Fact]
    public async Task AccessGuestAsync_ValidGuid_ReturnsTokensModel()
    {
        // Arrange
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

        // Act
        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var tokens = (TokensModel?) jsonObject?.data?.accessGuest?.ToObject<TokensModel>();

        // Assert
        Assert.NotNull(tokens);
        Assert.NotNull(tokens.AccessToken);
        Assert.NotNull(tokens.RefreshToken);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("invalid-guid-format")]
    public async Task AccessGuestAsync_InvalidGuid_ReturnsErrors(string guestId)
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

        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var errors = (JArray?) jsonObject?.errors;

        Assert.NotNull(errors);
        Assert.True(errors.Count > 0);
    }

    [Theory]
    [InlineData("invalid-email-format", null, "Yuiop12345")]
    [InlineData(null, "invalid-phone", "Yuiop12345")]
    [InlineData("test@gmail.com", null, "random-password")]
    [InlineData(null, null, "Yuiop12345")]
    public async Task LoginAsync_InvalidCredentials_ReturnsErrors(string email, string phone, string password)
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

        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var errors = (JArray?) jsonObject?.errors;

        Assert.NotNull(errors);
        Assert.True(errors.Count > 0);
    }
    
    [Theory]
    [InlineData("test@gmail.com", "+380953326869", "Yuiop12345")]
    [InlineData(null, "+380953326869", "Yuiop12345")]
    [InlineData("test@gmail.com", null, "Yuiop12345")]
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
        
        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var tokens = (TokensModel?) jsonObject?.data?.login?.ToObject<TokensModel>();

        Assert.NotNull(tokens);
        Assert.NotNull(tokens.AccessToken);
        Assert.NotNull(tokens.RefreshToken);
    }

    [Fact]
    public async Task RefreshUserTokenAsync_ValidTokensModel_ReturnsTokensModel()
    {
        var tokensModel = await CreateGuestAsync();
        var mutation = new
        {
            query = "mutation RefreshToken($model: TokensModelInput!) { refreshAccessToken(model: $model) { accessToken refreshToken }}",
            variables = new
            {
                model = new
                {
                    accessToken = tokensModel.AccessToken,
                    refreshToken = tokensModel.RefreshToken
                }
            }
        };
        
        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var tokens = (TokensModel?) jsonObject?.data?.refreshAccessToken?.ToObject<TokensModel>();

        Assert.NotNull(tokens);
        Assert.NotNull(tokens.AccessToken);
        Assert.NotNull(tokens.RefreshToken);
    }

    [Fact]
    public async Task RefreshAccessTokenAsync_NonExistingRefreshToken_ReturnsErrors()
    {
        var mutation = new
        {
            query = "mutation RefreshToken($model: TokensModelInput!) { refreshAccessToken(model: $model) { accessToken refreshToken }}",
            variables = new
            {
                model = new
                {
                    accessToken = "random-access-token",
                    refreshToken = "random-refresh-token"
                }
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var errors = (JArray?) jsonObject?.errors;

        Assert.NotNull(errors);
        Assert.True(errors.Count > 0);
    }

    private async Task<TokensModel?> CreateGuestAsync()
    {
        var mutation = new
        {
            query = @"
                mutation AccessGuest($guest: AccessGuestModelInput!) { 
                    accessGuest(guest: $guest) { 
                        accessToken, refreshToken 
                    } 
                }",
            variables = new
            {
                guest = new
                {
                    guestId = Guid.NewGuid()
                }
            }
        };

        var jsonObject = await SendGraphQlRequestAsync(mutation);
        var tokens = (TokensModel?) jsonObject?.data?.accessGuest?.ToObject<TokensModel>();

        return tokens;
    }
}