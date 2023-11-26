using ShoppingAssistantApi.Application.Models.Dtos;
using ShoppingAssistantApi.Application.Models.Identity;
using ShoppingAssistantApi.Application.Models.Operations;

namespace ShoppingAssistantApi.Application.IServices.Identity;

public interface IUserManager
{
    Task<TokensModel> AccessGuestAsync(AccessGuestModel guest, CancellationToken cancellationToken);

    Task<TokensModel> LoginAsync(AccessUserModel login, CancellationToken cancellationToken);

    Task<UserDto> AddToRoleAsync(string roleName, string userId, CancellationToken cancellationToken);

    Task<UserDto> RemoveFromRoleAsync(string roleName, string userId, CancellationToken cancellationToken);

    Task<UpdateUserModel> UpdateAsync(UserDto userDto, CancellationToken cancellationToken);

    Task<UserDto> UpdateUserByAdminAsync(string id, UserDto userDto, CancellationToken cancellationToken);

    Task<TokensModel> RefreshAccessTokenAsync(TokensModel tokensModel, CancellationToken cancellationToken);
}