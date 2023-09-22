using System.Text;
using Newtonsoft.Json;
using ShoppingAssistantApi.Application.Models.Identity;

namespace ShoppingAssistantApi.Tests.TestExtentions;

public static class AccessExtention
{
    public static async Task<TokensModel> Login(string email, string password, HttpClient httpClient)
    {
        var mutation = new
        {
            query = "mutation Login($login: AccessUserModelInput!) { login(login: $login) { accessToken refreshToken }}",
            variables = new
            {
                login = new
                {
                    email = email,
                    password = password
                }
            }
        };

        var jsonPayload = JsonConvert.SerializeObject(mutation);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await httpClient.PostAsync("graphql", content);
        var responseString = await response.Content.ReadAsStringAsync();
        var document = JsonConvert.DeserializeObject<dynamic>(responseString);

        return new TokensModel
        {
            AccessToken = (string)document.data.login.accessToken,
            RefreshToken = (string)document.data.login.refreshToken
        };
    }

    public static async Task<TokensModel> CreateGuest(string guestId, HttpClient httpClient)
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

        using var response = await httpClient.PostAsync("graphql", content);
        var responseString = await response.Content.ReadAsStringAsync();
        var document = JsonConvert.DeserializeObject<dynamic>(responseString);

        return new TokensModel
        {
            AccessToken = (string)document.data.accessGuest.accessToken,
            RefreshToken = (string)document.data.accessGuest.refreshToken
        };
    }
}