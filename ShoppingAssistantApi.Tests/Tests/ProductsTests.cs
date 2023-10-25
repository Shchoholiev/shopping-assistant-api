using System.Net;
using System.Net.Http.Json;
using System.Text;
using MongoDB.Bson;
using Newtonsoft.Json;
using ShoppingAssistantApi.Application.Models.CreateDtos;
using ShoppingAssistantApi.Tests.TestExtentions;

namespace ShoppingAssistantApi.Tests.Tests;

public class ProductsTests : TestsBase
{
    public ProductsTests(TestingFactory<Program> factory)
        : base(factory)
    {
    }
    
    [Fact]
    public async Task StreamDataToClient_ReturnsExpectedResponse()
    {
        await LoginAsync("wishlists@gmail.com", "Yuiop12345");
        // Arrange
        var wishlistId = "ab8c8c2d9edf39abcd1ef9ab";
        var message = new MessageCreateDto { Text = "I want new powerful laptop" };

        // Act
        var response = await _httpClient.PostAsJsonAsync($"http://127.0.0.1:5183/api/ProductsSearch/search/{wishlistId}", message);
        var responseContent = await response.Content.ReadAsStringAsync();
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseContent);
    }
    
    [Fact]
    public async Task StreamDataToClientFirstly_ReturnsExpectedResponse()
    {
        await LoginAsync("wishlists@gmail.com", "Yuiop12345");
        // Arrange
        var wishlistId = "ab7c8c2d9edf39abcd1ef9ab";
        var message = new MessageCreateDto { Text = "I want new powerful laptop" };

        // Act
        var response = await _httpClient.PostAsJsonAsync($"http://127.0.0.1:5183/api/ProductsSearch/search/{wishlistId}", message);
        var responseContent = await response.Content.ReadAsStringAsync();
        var sseEvents = responseContent.Split("\n\n", StringSplitOptions.RemoveEmptyEntries);
        bool foundMessageEvent = false;
        
        // Assert
        foreach (var sseEvent in sseEvents)
        {
            var sseParts = sseEvent.Split('\n');
            if (sseParts.Length >= 2)
            {
                var eventName = sseParts[0];
                var eventData = sseParts[1].Substring("data: ".Length);
                if (eventName == "event: Message")
                {
                    foundMessageEvent = true;
                    Assert.Equal("\"What are you looking for?\"", eventData);
                    break;
                }
            }
        }

        Assert.True(foundMessageEvent, "Message event not found");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseContent);
    }
}