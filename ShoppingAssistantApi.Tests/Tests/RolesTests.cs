using System.Net;
using System.Text;
using Xunit;
using ShoppingAssistantApi.Tests.TestExtentions;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using GreenDonut;

namespace ShoppingAssistantApi.Tests.Tests;

[Collection("Tests")]
public class RolesTests : IClassFixture<TestingFactory<Program>>
{
    private readonly HttpClient _httpClient;

    public RolesTests(TestingFactory<Program> factory)
    {
        _httpClient = factory.CreateClient();
        factory.InitialaizeData().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task AddToRoleAsync_ValidRoleName_ReturnsTokensModel()
    {
        var usersPage = await UserExtention.GetUsers(10, _httpClient);
        var mutation = new
        {
            query = "mutation AddToRole($roleName: String!, $id: String!) { addToRole(roleName: $roleName, id: $id) { accessToken, refreshToken }}",
            variables = new
            {
                roleName = "Admin",
                id = usersPage[0].Id,
            }
        };

        var jsonPayload = JsonConvert.SerializeObject(mutation);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseString = await response.Content.ReadAsStringAsync();
        var document = JsonConvert.DeserializeObject<dynamic>(responseString);

        var accessToken = (string)document.data.addToRole.accessToken;
        var refreshToken = (string)document.data.addToRole.refreshToken;

        Assert.NotNull(accessToken);
        Assert.NotNull(refreshToken);
    }


    [Theory]
    [InlineData("")]
    [InlineData("InvalidRole")]
    public async Task AddToRoleAsync_InvalidRoleName_ReturnsInternalServerError(string roleName)
    {
        var usersPage = await UserExtention.GetUsers(10, _httpClient);
        var mutation = new
        {
            query = "mutation AddToRole($roleName: String!, $id: String!) { addToRole(roleName: $roleName, id: $id) { accessToken, refreshToken }}",
            variables = new
            {
                roleName,
                id = usersPage[0].Id,
            }
        };

        var jsonPayload = JsonConvert.SerializeObject(mutation);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }


    [Fact]
    public async Task RemoveFromRoleAsync_ValidRoleName_ReturnsTokensModel()
    {
        var usersPage = await UserExtention.GetUsers(10, _httpClient);
        var mutation = new
        {
            query = "mutation RemoveFromRole($roleName: String!, $id: String!) { removeFromRole(roleName: $roleName, id: $id) { accessToken, refreshToken }}",
            variables = new
            {
                roleName = "Admin",
                id = usersPage[0].Id,
            }
        };

        var jsonPayload = JsonConvert.SerializeObject(mutation);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseString = await response.Content.ReadAsStringAsync();
        var document = JsonConvert.DeserializeObject<dynamic>(responseString);

        var accessToken = (string)document.data.removeFromRole.accessToken;
        var refreshToken = (string)document.data.removeFromRole.refreshToken;

        Assert.NotNull(accessToken);
        Assert.NotNull(refreshToken);
    }

    [Theory]
    [InlineData("")]
    [InlineData("InvalidRole")]
    public async Task RemoveFromRoleAsync_InvalidRoleName_ReturnsInternalServerError(string roleName)
    {
        var usersPage = await UserExtention.GetUsers(10, _httpClient);
        var mutation = new
        {
            query = "mutation RemoveFromRole($roleName: String!, $id: String!) { removeFromRole(roleName: $roleName, id: $id) { accessToken, refreshToken }}",
            variables = new
            {
                roleName,
                id = usersPage[0].Id,
            }
        };

        var jsonPayload = JsonConvert.SerializeObject(mutation);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Theory]
    [InlineData("User")]
    [InlineData(null)]
    public async Task AddRole_InvalidRoleName_ReturnsInternalServerError(string roleName)
    {
        var mutation = new
        {
            query = "mutation AddRole ($dto: RoleCreateDtoInput!){ addRole (roleDto: $dto) { id, name }} ",
            variables = new
            {
                dto = new
                {
                    name = roleName
                }
            }
        };

        var jsonPayload = JsonConvert.SerializeObject(mutation);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task GetRolesPageAsync_ValidPageNumberAndSize_ReturnsRolesPagedList()
    {
        var tokensModel = await AccessExtention.Login("mykhailo.bilodid@nure.ua", "Yuiop12345", _httpClient);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokensModel.AccessToken);
        var query = new
        {
            query = "query RolesPage($pageNumber: Int!, $pageSize: Int!) { rolesPage(pageNumber: $pageNumber, pageSize: $pageSize) { items { id, name } }}",
            variables = new
            {
                pageNumber = 1,
                pageSize = 3
            }
        };

        var jsonPayload = JsonConvert.SerializeObject(query);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseString = await response.Content.ReadAsStringAsync();
        var document = JsonConvert.DeserializeObject<dynamic>(responseString);

        var items = document.data.rolesPage.items;
        Assert.NotEmpty(items);
    }
}