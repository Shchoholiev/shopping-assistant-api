using ShoppingAssistantApi.Application.Models.CreateDtos;
using ShoppingAssistantApi.Application.Models.Dtos;

namespace ShoppingAssistantApi.Application.IServices;

public interface IWishlistsService
{
    Task<WishlistDto> StartPersonalWishlistAsync(WishlistCreateDto dto, CancellationToken cancellationToken);

    Task<MessageDto> AddMessageToPersonalWishlistAsync(string wishlistId, MessageCreateDto dto, CancellationToken cancellationToken);
}
