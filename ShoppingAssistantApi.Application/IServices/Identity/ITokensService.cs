using ShoppingAssistantApi.Application.Models.Identity;
using System.Security.Claims;

namespace ShoppingAssistantApi.Application.IServices.Identity;

public interface ITokensService
{
    string GenerateAccessToken(IEnumerable<Claim> claims);

    string GenerateRefreshToken();

    Task<TokensModel> RefreshUserAsync(TokensModel tokensModel, CancellationToken cancellationToken);
}