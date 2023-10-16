using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using ShoppingAssistantApi.Application.IServices.Identity;

namespace ShoppingAssistantApi.Infrastructure.Services.Identity;

public class TokensService : ITokensService
{
    private readonly IConfiguration _configuration;

    private readonly ILogger _logger;

    public TokensService(
        IConfiguration configuration, 
        ILogger<TokensService> logger)
    {
        this._configuration = configuration;
        this._logger = logger;
    }

    public string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        var tokenOptions = GetTokenOptions(claims);
        var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

        this._logger.LogInformation($"Generated new access token.");

        return tokenString;
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        var refreshToken = Convert.ToBase64String(randomNumber);

        this._logger.LogInformation($"Generated new refresh token.");

        return refreshToken;
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetValue<string>("JsonWebTokenKeys:IssuerSigningKey"))),
            ValidateLifetime = false
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
        var jwtSecurityToken = securityToken as JwtSecurityToken;
        if (jwtSecurityToken == null 
            || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");

        this._logger.LogInformation($"Returned data from expired access token.");

        return principal;
    }

    private JwtSecurityToken GetTokenOptions(IEnumerable<Claim> claims)
    {
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration.GetValue<string>("JsonWebTokenKeys:IssuerSigningKey")));
        var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        var tokenOptions = new JwtSecurityToken(
            issuer: _configuration.GetValue<string>("JsonWebTokenKeys:ValidIssuer"),
            audience: _configuration.GetValue<string>("JsonWebTokenKeys:ValidAudience"),
            expires: DateTime.UtcNow.AddMinutes(15),
            claims: claims,
            signingCredentials: signinCredentials
        );

        return tokenOptions;
    }
}