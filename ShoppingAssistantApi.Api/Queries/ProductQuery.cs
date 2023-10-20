using HotChocolate.Authorization;
using ShoppingAssistantApi.Application.IServices;
using ShoppingAssistantApi.Domain.Entities;

namespace ShoppingAssistantApi.Api.Queries;

[ExtendObjectType(OperationTypeNames.Query)]
public class ProductQuery
{
    [Authorize]
    public IAsyncEnumerable<string> GetProductFromSearch(Message message, CancellationToken cancellationToken,
        [Service] IProductService productService)
        => productService.GetProductFromSearch(message, cancellationToken);
    
    [Authorize]
    public IAsyncEnumerable<string> GetRecommendationsForProductFromSearchStream(Message message, CancellationToken cancellationToken,
        [Service] IProductService productService)
        => productService.GetRecommendationsForProductFromSearchStream(message, cancellationToken);

}