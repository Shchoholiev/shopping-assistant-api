using System.Collections.ObjectModel;
using ShoppingAssistantApi.Application.Models.Dtos;
using ShoppingAssistantApi.Application.Models.OpenAi;
using ShoppingAssistantApi.Application.Models.ProductSearch;
using ShoppingAssistantApi.Domain.Entities;

namespace ShoppingAssistantApi.Application.IServices;

public interface IProductService
{
    IAsyncEnumerable<(List<ProductName> ProductNames, WishlistDto Wishlist)> StartNewSearchAndReturnWishlist(Message message, CancellationToken cancellationToken);
    
    Task<List<string>> GetProductFromSearch(Message message, CancellationToken cancellationToken);
    
    Task<List<string>> GetRecommendationsForProductFromSearch(Message message, CancellationToken cancellationToken);
}