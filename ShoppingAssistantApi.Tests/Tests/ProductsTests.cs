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
        // Arrange
        var wishlistId = "your_wishlist_id";
        var message = new MessageCreateDto { Text = "Your message text" };

        // Act
        var response = await _httpClient.PostAsJsonAsync($"http://localhost:5183/api/products/search/{"ab79cde6f69abcd3efab65cd"}", message);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Додайте додаткові перевірки на відповідь, якщо необхідно
    }
}