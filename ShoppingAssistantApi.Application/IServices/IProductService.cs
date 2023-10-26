using ShoppingAssistantApi.Application.Models.CreateDtos;
using ShoppingAssistantApi.Application.Models.Dtos;
using ShoppingAssistantApi.Application.Models.ProductSearch;
using ShoppingAssistantApi.Domain.Entities;

namespace ShoppingAssistantApi.Application.IServices;

public interface IProductService
{
    IAsyncEnumerable<ServerSentEvent> SearchProductAsync(string wishlistId, MessageCreateDto message, CancellationToken cancellationToken);
    
}