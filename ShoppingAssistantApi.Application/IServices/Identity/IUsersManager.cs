using ShoppingAssistantApi.Application.Models.Dtos;
using ShoppingAssistantApi.Application.Models.Identity;
using ShoppingAssistantApi.Application.Models.Operations;

namespace ShoppingAssistantApi.Application.IServices.Identity;

public interface IUserManager
{
    Task<TokensModel> AccessGuestAsync(AccessGuestModel guest, CancellationToken cancellationToken);

    Task<TokensModel> LoginAsync(AccessUserModel login, CancellationToken cancellationToken);

    Task<TokensModel> AddToRoleAsync(string roleName, string id, CancellationToken cancellationToken);

    Task<TokensModel> RemoveFromRoleAsync(string roleName, string id, CancellationToken cancellationToken);

    Task<UpdateUserModel> UpdateAsync(UserDto userDto, CancellationToken cancellationToken);

    Task<UpdateUserModel> UpdateUserByAdminAsync(string id, UserDto userDto, CancellationToken cancellationToken);
}