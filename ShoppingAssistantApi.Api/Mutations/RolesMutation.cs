using ShoppingAssistantApi.Application.IServices.Identity;
using ShoppingAssistantApi.Application.IServices;
using ShoppingAssistantApi.Application.Models.CreateDtos;
using ShoppingAssistantApi.Application.Models.Dtos;
using ShoppingAssistantApi.Application.Models.Identity;

namespace ShoppingAssistantApi.Api.Mutations;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class RolesMutation
{
    public Task<TokensModel> AddToRoleAsync(string roleName, string id, CancellationToken cancellationToken,
        [Service] IUserManager userManager)
        => userManager.AddToRoleAsync(roleName, id, cancellationToken);

    public Task<TokensModel> RemoveFromRoleAsync(string roleName, string id, CancellationToken cancellationToken,
        [Service] IUserManager userManager)
        => userManager.RemoveFromRoleAsync(roleName, id, cancellationToken);

    public Task<RoleDto> AddRole(RoleCreateDto roleDto, CancellationToken cancellationToken,
        [Service] IRolesService rolesService)
        => rolesService.AddRoleAsync(roleDto, cancellationToken);
}