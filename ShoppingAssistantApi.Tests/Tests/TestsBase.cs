using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using ShoppingAssistantApi.Application.Models.Dtos;
using ShoppingAssistantApi.Tests.TestExtentions;

namespace ShoppingAssistantApi.Tests.Tests;

public class TestsBase: IClassFixture<TestingFactory<Program>>
{
    private protected HttpClient _httpClient;

    public TestsBase(TestingFactory<Program> factory)
    {
        _httpClient = factory.CreateClient();
        factory.InitialaizeDatabase();
    }
    
    public async Task LoginAsync(string email, string password)
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

        var jsonObject = await SendGraphQlRequestAsync(mutation);
        
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (string?) jsonObject?.data?.login?.accessToken);
    }

    public async Task<dynamic?> SendGraphQlRequestAsync(object request)
    {
        var jsonPayload = JsonConvert.SerializeObject(request);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("graphql", content);
        var responseString = await response.Content.ReadAsStringAsync();
        Console.WriteLine(responseString);

        var jsonObject = JsonConvert.DeserializeObject<dynamic>(responseString);

        return jsonObject;
    }
    
    public async Task<UserDto> GetCurrentUserAsync()
    {
        var query = new
        {
            query = "query CurrentUser { currentUser { id, guestId, phone, email, roles { id, name }}}",
            variables = new { }
        };

        var jsonObject = await SendGraphQlRequestAsync(query);
        var user = (UserDto?) jsonObject?.data?.currentUser?.ToObject<UserDto>();

        return user;
    }
}