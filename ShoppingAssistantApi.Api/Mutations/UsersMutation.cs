using ShoppingAssistantApi.Application.IServices.Identity;
using ShoppingAssistantApi.Application.Models.Dtos;
using ShoppingAssistantApi.Application.Models.Operations;
using HotChocolate.Authorization;

namespace ShoppingAssistantApi.Api.Mutations;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class UsersMutation
{
    [Authorize]
    public Task<UpdateUserModel> UpdateUserAsync(UserDto userDto, CancellationToken cancellationToken,
        [Service] IUserManager userManager)
        => userManager.UpdateAsync(userDto, cancellationToken);

    [Authorize]
    public Task<UserDto> UpdateUserByAdminAsync(string id, UserDto userDto, CancellationToken cancellationToken,
        [Service] IUserManager userManager)
        => userManager.UpdateUserByAdminAsync(id, userDto, cancellationToken);
        
    [Authorize]
    public Task<UserDto> AddToRoleAsync(string roleName, string userId, CancellationToken cancellationToken,
        [Service] IUserManager userManager)
        => userManager.AddToRoleAsync(roleName, userId, cancellationToken);

    [Authorize]
    public Task<UserDto> RemoveFromRoleAsync(string roleName, string userId, CancellationToken cancellationToken,
        [Service] IUserManager userManager)
        => userManager.RemoveFromRoleAsync(roleName, userId, cancellationToken);
}