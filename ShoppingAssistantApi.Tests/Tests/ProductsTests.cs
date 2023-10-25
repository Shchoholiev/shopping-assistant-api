﻿using System.Net;
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

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}