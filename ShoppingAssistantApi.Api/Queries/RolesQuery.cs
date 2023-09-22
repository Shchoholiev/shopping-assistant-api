using ShoppingAssistantApi.Application.IServices;
using ShoppingAssistantApi.Application.Models.Dtos;
using ShoppingAssistantApi.Application.Paging;
using HotChocolate.Authorization;

namespace ShoppingAssistantApi.Api.Queries;

[ExtendObjectType(OperationTypeNames.Query)]
public class RolesQuery
{
    [Authorize]
    public Task<PagedList<RoleDto>> GetRolesPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken,
        [Service] IRolesService service)
        => service.GetRolesPageAsync(pageNumber, pageSize, cancellationToken);
}
