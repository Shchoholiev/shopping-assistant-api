using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using ShoppingAssistantApi.Application.Models.Dtos;

namespace ShoppingAssistantApi.Tests.TestExtentions;

public static class UserExtention
{
    public static async Task<UserDto> GetCurrentUser(HttpClient httpClient)
    {
        var query = new
        {
            query = "query CurrentUser { currentUser { id, guestId, phone, email, refreshToken, refreshTokenExpiryDate, roles { id, name }}}",
            variables = new { }
        };

        var jsonPayload = JsonConvert.SerializeObject(query);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await httpClient.PostAsync("graphql", content);
        var responseString = await response.Content.ReadAsStringAsync();
        var document = JsonConvert.DeserializeObject<dynamic>(responseString);
        return JsonConvert.DeserializeObject<UserDto>(document.data.currentUser.ToString());
    }

    public static async Task<List<UserDto>> GetUsers(int amount, HttpClient httpClient)
    {
        var accessToken = await AccessExtention.Login("mykhailo.bilodid@nure.ua", "Yuiop12345", httpClient);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.AccessToken);

        var query = new
        {
            query = "query UsersPage($pageNumber: Int!, $pageSize: Int!) { usersPage(pageNumber: $pageNumber, pageSize: $pageSize) { items { id, email, phone }}}",
            variables = new
            {
                pageNumber = 1,
                pageSize = amount
            }
        };

        var jsonPayload = JsonConvert.SerializeObject(query);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await httpClient.PostAsync("graphql", content);
        var responseString = await response.Content.ReadAsStringAsync();
        var document = JsonConvert.DeserializeObject<dynamic>(responseString);
        return JsonConvert.DeserializeObject<List<UserDto>>(document.data.usersPage.items.ToString());
    }
}