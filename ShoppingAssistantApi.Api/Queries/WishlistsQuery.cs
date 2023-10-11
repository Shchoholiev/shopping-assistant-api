using HotChocolate.Authorization;
using ShoppingAssistantApi.Application.IServices;
using ShoppingAssistantApi.Application.Models.Dtos;
using ShoppingAssistantApi.Application.Paging;

namespace ShoppingAssistantApi.Api.Queries;

[ExtendObjectType(OperationTypeNames.Query)]
public class WishlistsQuery
{
    [Authorize]
    public Task<PagedList<WishlistDto>> GetPersonalWishlistsPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken,
    [Service] IWishlistsService wishlistsService)
    => wishlistsService.GetPersonalWishlistsPageAsync(pageNumber, pageSize, cancellationToken);

    [Authorize]
    public Task<WishlistDto> GetPersonalWishlistAsync(string wishlistId, CancellationToken cancellationToken,
    [Service] IWishlistsService wishlistsService)
    => wishlistsService.GetPersonalWishlistAsync(wishlistId, cancellationToken);
}
