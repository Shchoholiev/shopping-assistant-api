using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ShoppingAssistantApi.Application.IServices;
using ShoppingAssistantApi.Application.Models.CreateDtos;

namespace ShoppingAssistantApi.Api.Controllers;

public class ProductsSearchController : BaseController
{
    private readonly IProductService _productService;

    public ProductsSearchController(IProductService productService)
    {
        _productService = productService;
    }
    
    [HttpPost("search/{wishlistId}")]
    public async Task StreamDataToClient(string wishlistId, [FromBody]MessageCreateDto message, CancellationToken cancellationToken)
    {
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