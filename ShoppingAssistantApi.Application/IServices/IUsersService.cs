using ShoppingAssistantApi.Application.Models.Dtos;
using ShoppingAssistantApi.Application.Paging;

namespace ShoppingAssistantApi.Application.IServices;

public interface IUsersService
{
    Task AddUserAsync(UserDto dto, CancellationToken cancellationToken);

    Task<PagedList<UserDto>> GetUsersPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);

    Task<UserDto> GetUserAsync(string id, CancellationToken cancellationToken);

    Task UpdateUserAsync(UserDto dto, CancellationToken cancellationToken);

    Task DeletePersonalUserAsync(string guestId, CancellationToken cancellationToken);
}
