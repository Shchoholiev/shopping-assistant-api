using ShoppingAssistantApi.Application.Models.CreateDtos;
using ShoppingAssistantApi.Application.Models.Dtos;
using ShoppingAssistantApi.Application.Paging;

namespace ShoppingAssistantApi.Application.IServices;

public interface IRolesService
{
    Task<RoleDto> AddRoleAsync(RoleCreateDto dto, CancellationToken cancellationToken);

    Task<PagedList<RoleDto>> GetRolesPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
}