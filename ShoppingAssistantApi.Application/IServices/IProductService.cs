using ShoppingAssistantApi.Application.Models.CreateDtos;
using ShoppingAssistantApi.Application.Models.Dtos;
using ShoppingAssistantApi.Application.Models.ProductSearch;
using ShoppingAssistantApi.Domain.Entities;

namespace ShoppingAssistantApi.Application.IServices;

public interface IProductService
{
    IAsyncEnumerable<(List<ProductName> ProductNames, WishlistDto Wishlist)> StartNewSearchAndReturnWishlist(Message message, CancellationToken cancellationToken);

    IAsyncEnumerable<ServerSentEvent> SearchProductAsync(string wishlistId, MessageCreateDto message, CancellationToken cancellationToken);

    IAsyncEnumerable<string> GetProductFromSearch(Message message, CancellationToken cancellationToken);

    IAsyncEnumerable<string> GetRecommendationsForProductFromSearchStream(Message message,
        CancellationToken cancellationToken);
}