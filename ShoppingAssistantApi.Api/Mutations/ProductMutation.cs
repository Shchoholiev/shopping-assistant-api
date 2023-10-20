using ShoppingAssistantApi.Application.IServices;
using ShoppingAssistantApi.Application.Models.Dtos;
using ShoppingAssistantApi.Application.Models.ProductSearch;
using ShoppingAssistantApi.Domain.Entities;
using ShoppingAssistantApi.Infrastructure.Services;

namespace ShoppingAssistantApi.Api.Mutations;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class ProductMutation
{
    public IAsyncEnumerable<(List<ProductName> ProductNames, WishlistDto Wishlist)> StartNewSearchAndReturnWishlist(
        Message message, CancellationToken cancellationToken, [Service] IProductService productService)
        => productService.StartNewSearchAndReturnWishlist(message, cancellationToken);
}