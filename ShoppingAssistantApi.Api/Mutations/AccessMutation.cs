using ShoppingAssistantApi.Application.IServices.Identity;
using ShoppingAssistantApi.Application.Models.Identity;

namespace ShoppingAssistantApi.Api.Mutations;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class AccessMutation
{
    public Task<TokensModel> LoginAsync(AccessUserModel login, CancellationToken cancellationToken,
        [Service] IUserManager userManager)
        => userManager.LoginAsync(login, cancellationToken);

    public Task<TokensModel> AccessGuestAsync(AccessGuestModel guest, CancellationToken cancellationToken,
        [Service] IUserManager userManager)
        => userManager.AccessGuestAsync(guest, cancellationToken);

    public Task<TokensModel> RefreshAccessTokenAsync(TokensModel model, CancellationToken cancellationToken,
        [Service] IUserManager userManager)
        => userManager.RefreshAccessTokenAsync(model, cancellationToken);
}