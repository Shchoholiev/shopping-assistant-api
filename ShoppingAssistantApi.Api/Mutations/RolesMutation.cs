using HotChocolate.Authorization;
using ShoppingAssistantApi.Application.IServices;
using ShoppingAssistantApi.Application.Models.CreateDtos;
using ShoppingAssistantApi.Application.Models.Dtos;

namespace ShoppingAssistantApi.Api.Mutations;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class RolesMutation
{
    [Authorize]
    public Task<RoleDto> AddRole(RoleCreateDto roleDto, CancellationToken cancellationToken,
        [Service] IRolesService rolesService)
        => rolesService.AddRoleAsync(roleDto, cancellationToken);
}