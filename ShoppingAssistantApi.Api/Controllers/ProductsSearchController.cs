using HotChocolate.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ShoppingAssistantApi.Application.IServices;
using ShoppingAssistantApi.Application.Models.CreateDtos;
using ShoppingAssistantApi.Application.Models.Dtos;
using ShoppingAssistantApi.Domain.Enums;

namespace ShoppingAssistantApi.Api.Controllers;

[Authorize]
public class ProductsSearchController : BaseController
{
    private readonly IProductService _productService;

    private readonly IWishlistsService _wishlistsService;

    public ProductsSearchController(IProductService productService, IWishlistsService wishlistsService)
    {
        _productService = productService;
        _wishlistsService = wishlistsService;
    }
    
    [HttpPost("search/{wishlistId}")]
    public async Task StreamDataToClient(string wishlistId, [FromBody]MessageCreateDto message, CancellationToken cancellationToken)
    {
        var dto = new MessageDto()
        {
            Text = message.Text,
            Role = MessageRoles.User.ToString(),
        };
        await _wishlistsService.AddMessageToPersonalWishlistAsync(wishlistId, dto, cancellationToken);

        Response.Headers.Add("Content-Type", "text/event-stream");
        Response.Headers.Add("Cache-Control", "no-cache");
        Response.Headers.Add("Connection", "keep-alive");
        
        var result = _productService.SearchProductAsync(wishlistId, message, cancellationToken);
        
        await foreach (var sse in result)
        {
            var chunk = JsonConvert.SerializeObject(sse.Data);
            
            var serverSentEvent = $"event: {sse.Event}\ndata: {chunk}\n\n";
            
            await Response.WriteAsync(serverSentEvent);
            await Response.Body.FlushAsync();
        }
    }
    
}