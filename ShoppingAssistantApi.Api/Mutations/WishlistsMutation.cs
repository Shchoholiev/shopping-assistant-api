using ShoppingAssistantApi.Application.IServices;
using ShoppingAssistantApi.Application.Models.CreateDtos;
using ShoppingAssistantApi.Application.Models.Dtos;

namespace ShoppingAssistantApi.Api.Mutations;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class WishlistsMutation
{
    public Task<WishlistDto> StartPersonalWishlist(WishlistCreateDto dto, CancellationToken cancellationToken,
        [Service] IWishlistsService wishlistsService)
        => wishlistsService.StartPersonalWishlistAsync(dto, cancellationToken);

    public Task<MessageDto> AddMessageToPersonalWishlist(string wishlistId, MessageCreateDto dto, CancellationToken cancellationToken,
        [Service] IWishlistsService wishlistsService)
        => wishlistsService.AddMessageToPersonalWishlistAsync(wishlistId, dto, cancellationToken);

    public Task<WishlistDto> DeletePersonalWishlistAsync(string wishlistId, CancellationToken cancellationToken,
        [Service] IWishlistsService wishlistsService)
        => wishlistsService.DeletePersonalWishlistAsync(wishlistId, cancellationToken);
}
