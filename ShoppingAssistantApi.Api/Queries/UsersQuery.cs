using HotChocolate.Authorization;
using ShoppingAssistantApi.Application.GlobalInstances;
using ShoppingAssistantApi.Application.IServices;
using ShoppingAssistantApi.Application.Models.Dtos;
using ShoppingAssistantApi.Application.Paging;

namespace ShoppingAssistantApi.Api.Queries;

[ExtendObjectType(OperationTypeNames.Query)]
public class UsersQuery
{
    [Authorize]
    public Task<UserDto> GetUserAsync(string id, CancellationToken cancellationToken,
    [Service] IUsersService usersService)
    => usersService.GetUserAsync(id, cancellationToken);

    [Authorize]
    public Task<UserDto> GetCurrentUserAsync(CancellationToken cancellationToken,
    [Service] IUsersService usersService)
    => usersService.GetUserAsync(GlobalUser.Id.ToString(), cancellationToken);

    [Authorize]
    public Task<PagedList<UserDto>> GetUsersPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken,
    [Service] IUsersService usersService)
    => usersService.GetUsersPageAsync(pageNumber, pageSize, cancellationToken);
}