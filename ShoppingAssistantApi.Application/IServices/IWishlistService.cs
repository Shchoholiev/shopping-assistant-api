using ShoppingAssistantApi.Application.Models.CreateDtos;
using ShoppingAssistantApi.Application.Models.Dtos;
using ShoppingAssistantApi.Application.Paging;

namespace ShoppingAssistantApi.Application.IServices;

public interface IWishlistsService
{
    Task<WishlistDto> StartPersonalWishlistAsync(WishlistCreateDto dto, CancellationToken cancellationToken);

    Task<MessageDto> AddMessageToPersonalWishlistAsync(string wishlistId, MessageCreateDto dto, CancellationToken cancellationToken);

    Task<PagedList<WishlistDto>> GetPersonalWishlistsPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);

    Task<WishlistDto> GetPersonalWishlistAsync(string wishlistId, CancellationToken cancellationToken);

    Task<WishlistDto> DeletePersonalWishlistAsync(string wishlistId, CancellationToken cancellationToken);
}
