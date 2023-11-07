using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using ShoppingAssistantApi.Application.Exceptions;
using ShoppingAssistantApi.Application.GlobalInstances;
using ShoppingAssistantApi.Application.IRepositories;
using ShoppingAssistantApi.Application.IServices.Identity;
using ShoppingAssistantApi.Application.Models.Dtos;
using ShoppingAssistantApi.Application.Models.Identity;
using ShoppingAssistantApi.Application.Models.Operations;
using ShoppingAssistantApi.Domain.Entities;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace ShoppingAssistantApi.Infrastructure.Services.Identity;

public class UserManager : ServiceBase, IUserManager
{
    private readonly IUsersRepository _usersRepository;

    private readonly IPasswordHasher _passwordHasher;

    private readonly ITokensService _tokensService;

    private readonly IRolesRepository _rolesRepository;
    
    private readonly IRefreshTokensRepository _refreshTokensRepository;

    private readonly IMapper _mapper;

    private readonly ILogger _logger;

    public UserManager(
        IUsersRepository usersRepository, 
        IPasswordHasher passwordHasher, 
        ITokensService tokensService, 
        IRolesRepository rolesRepository,
        IRefreshTokensRepository refreshTokensRepository,
        IMapper mapper, 
        ILogger<UserManager> logger)
    {
        this._usersRepository = usersRepository;
        this._logger = logger;
        this._passwordHasher = passwordHasher;
        this._tokensService = tokensService;
        this._mapper = mapper;
        this._rolesRepository = rolesRepository;
        this._refreshTokensRepository = refreshTokensRepository;
    }

    public async Task<TokensModel> LoginAsync(AccessUserModel login, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Logging in user with email: {login.Email} and phone: {login.Phone}.");

        var user = string.IsNullOrEmpty(login.Phone)
            ? await this._usersRepository.GetUserAsync(u => u.Email == login.Email && u.IsDeleted == false, cancellationToken)
            : await this._usersRepository.GetUserAsync(u => u.Phone == login.Phone && u.IsDeleted == false, cancellationToken);
        if (user == null)
        {
            throw new EntityNotFoundException<User>();
        }

        if (!this._passwordHasher.Check(login.Password, user.PasswordHash))
        {
            throw new InvalidDataException("Invalid password!");
        }

        var refreshToken = await AddRefreshToken(user.Id, cancellationToken);

        var tokens = this.GetUserTokens(user, refreshToken);

        this._logger.LogInformation($"Logged in user with email: {login.Email} and phone: {login.Phone}.");

        return tokens;
    }

    public async Task<TokensModel> AccessGuestAsync(AccessGuestModel guest, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Logging in / Registering guest with guest id: {guest.GuestId}.");

        var user = await this._usersRepository.GetUserAsync(x => x.GuestId == guest.GuestId && x.IsDeleted == false, cancellationToken);

        if (user == null)
        {
            var role = await this._rolesRepository.GetRoleAsync(r => r.Name == "Guest" && r.IsDeleted == false, cancellationToken);
            user = new User
            {
                GuestId = guest.GuestId,
                Roles = new List<Role> { role },
                CreatedDateUtc = DateTime.UtcNow,
                CreatedById = ObjectId.Empty // Default value for all new users
            };

            await this._usersRepository.AddAsync(user, cancellationToken);

            this._logger.LogInformation($"Created guest with guest id: {guest.GuestId}.");
        }

        var refreshToken = await AddRefreshToken(user.Id, cancellationToken);
        var tokens = this.GetUserTokens(user, refreshToken);

        this._logger.LogInformation($"Logged in guest with guest id: {guest.GuestId}.");

        return tokens;
    }

    public async Task<TokensModel> RefreshAccessTokenAsync(TokensModel tokensModel, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Refreshing access token.");

        var principal = _tokensService.GetPrincipalFromExpiredToken(tokensModel.AccessToken);
        var userId = ParseObjectId(principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);

        var refreshTokenModel = await this._refreshTokensRepository
            .GetOneAsync(r => 
                r.Token == tokensModel.RefreshToken 
                && r.CreatedById == userId
                && r.IsDeleted == false, cancellationToken);
        if (refreshTokenModel == null || refreshTokenModel.ExpiryDateUTC < DateTime.UtcNow)
        {
            throw new SecurityTokenExpiredException();
        }

        var refreshToken = refreshTokenModel.Token;

        // Update Refresh token if it expires in less than 7 days to keep user constantly logged in if he uses the app
        if (refreshTokenModel.ExpiryDateUTC.AddDays(-7) < DateTime.UtcNow)
        {
            await _refreshTokensRepository.DeleteAsync(refreshTokenModel, cancellationToken);
            
            var newRefreshToken = await AddRefreshToken(userId, cancellationToken);
            refreshToken = newRefreshToken.Token;
        }

        var tokens = new TokensModel
        {
            AccessToken = _tokensService.GenerateAccessToken(principal.Claims),
            RefreshToken = refreshToken
        };

        this._logger.LogInformation($"Refreshed access token.");

        return tokens;
    }

    public async Task<UserDto> AddToRoleAsync(string roleName, string userId, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Adding Role: {roleName} to User with Id: {userId}.");

        var role = await this._rolesRepository.GetRoleAsync(r => r.Name == roleName && r.IsDeleted == false, cancellationToken);
        if (role == null)
        {
            throw new EntityNotFoundException<Role>();
        }

        var userObjectId = ParseObjectId(userId);
        var user = await this._usersRepository.GetUserAsync(userObjectId, cancellationToken);
        if (user == null)
        {
            throw new EntityNotFoundException<User>();
        }

        user.Roles.Add(role);
        var updatedUser = await this._usersRepository.UpdateUserAsync(user, cancellationToken);
        var userDto = this._mapper.Map<UserDto>(updatedUser);

        this._logger.LogInformation($"Added Role: {roleName} to User with Id: {userId}.");

        return userDto;
    }

    public async Task<UserDto> RemoveFromRoleAsync(string roleName, string userId, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Removing Role: {roleName} from User with Id: {userId}.");

        var role = await this._rolesRepository.GetRoleAsync(r => r.Name == roleName && r.IsDeleted == false, cancellationToken);
        if (role == null)
        {
            throw new EntityNotFoundException<Role>();
        }

        var userObjectId = ParseObjectId(userId);
        var user = await this._usersRepository.GetUserAsync(userObjectId, cancellationToken);
        if (user == null)
        {
            throw new EntityNotFoundException<User>();
        }

        var deletedRole = user.Roles.Find(x => x.Name == role.Name);
        user.Roles.Remove(deletedRole);

        var updatedUser = await this._usersRepository.UpdateUserAsync(user, cancellationToken);
        var userDto = this._mapper.Map<UserDto>(updatedUser);

        this._logger.LogInformation($"Removed Role: {roleName} from User with Id: {userId}.");

        return userDto;
    }

    public async Task<UpdateUserModel> UpdateAsync(UserDto userDto, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Updating user with id: {GlobalUser.Id}.");

        var user = await this._usersRepository.GetUserAsync(x => x.Id == GlobalUser.Id && x.IsDeleted == false, cancellationToken);
        if (user == null)
        {
            throw new EntityNotFoundException<User>();
        }

        await ValidateUserAsync(userDto, user, cancellationToken);   

        this._mapper.Map(userDto, user);
        if (!string.IsNullOrEmpty(userDto.Password))
        {
            user.PasswordHash = this._passwordHasher.Hash(userDto.Password);
        }
        await CheckAndUpgradeToUserAsync(user, cancellationToken);

        var updatedUser = await this._usersRepository.UpdateUserAsync(user, cancellationToken);

        var refreshToken = await AddRefreshToken(user.Id, cancellationToken);
        var tokens = this.GetUserTokens(user, refreshToken);

        var updatedUserDto = this._mapper.Map<UserDto>(updatedUser);

        this._logger.LogInformation($"Update user with id: {GlobalUser.Id}.");

        return new UpdateUserModel() 
        { 
            Tokens = tokens, 
            User = updatedUserDto
        };
    }

    public async Task<UserDto> UpdateUserByAdminAsync(string id, UserDto userDto, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Admin updating User with Id: {id}.");

        var userObjectId = ParseObjectId(id);
        var user = await this._usersRepository.GetUserAsync(userObjectId, cancellationToken);
        if (user == null)
        {
            throw new EntityNotFoundException<User>();
        }

        await ValidateUserAsync(userDto, user, cancellationToken);   

        this._mapper.Map(userDto, user);
        var updatedUser = await this._usersRepository.UpdateUserAsync(user, cancellationToken);

        var updatedUserDto = this._mapper.Map<UserDto>(updatedUser);
        
        this._logger.LogInformation($"Admin updated User with Id: {id}.");

        return updatedUserDto;
    }

    private async Task<RefreshToken> AddRefreshToken(ObjectId userId, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Adding new refresh token for user with Id : {userId}.");

        var refreshToken = new RefreshToken
        {
            Token = _tokensService.GenerateRefreshToken(),
            ExpiryDateUTC = DateTime.UtcNow.AddDays(30),
            CreatedById = userId,
            CreatedDateUtc = DateTime.UtcNow
        };

        await this._refreshTokensRepository.AddAsync(refreshToken, cancellationToken);

        this._logger.LogInformation($"Added new refresh token.");

        return refreshToken;
    }

    private TokensModel GetUserTokens(User user, RefreshToken refreshToken)
    {
        var claims = this.GetClaims(user);
        var accessToken = this._tokensService.GenerateAccessToken(claims);

        this._logger.LogInformation($"Returned new access and refresh tokens.");

        return new TokensModel
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
        };
    }

    private IEnumerable<Claim> GetClaims(User user)
    {
        var claims = new List<Claim>()
            {
                new (ClaimTypes.NameIdentifier, user.Id.ToString()),
                new (ClaimTypes.Email, user.Email ?? string.Empty),
                new (ClaimTypes.MobilePhone, user.Phone ?? string.Empty),
            };

        foreach (var role in user.Roles)
        {
            claims.Add(new (ClaimTypes.Role, role.Name));
        }

        this._logger.LogInformation($"Returned claims for User with Id: {user.Id}.");

        return claims;
    }

    private async Task CheckAndUpgradeToUserAsync(User user, CancellationToken cancellationToken)
    {
        if (user.Roles.Any(x => x.Name == "Guest" && x.IsDeleted == false) && !user.Roles.Any(x => x.Name == "User" && x.IsDeleted == false))
        {
            if (!string.IsNullOrEmpty(user.PasswordHash) && (!string.IsNullOrEmpty(user.Email) || !string.IsNullOrEmpty(user.Phone)))
            {
                var role = await this._rolesRepository.GetRoleAsync(x => x.Name == "User" && x.IsDeleted == false, cancellationToken);
                user.Roles.Add(role);
            }
        }
    }

    private async Task ValidateUserAsync(UserDto userDto, User user, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(userDto.Email)) 
        {
            ValidateEmail(userDto.Email);
            if (userDto.Email != user.Email 
                && await this._usersRepository.ExistsAsync(x => x.Email == userDto.Email && x.IsDeleted == false, cancellationToken))
            {
                throw new EntityAlreadyExistsException<User>("email", userDto.Email);
            }
        }

        if (!string.IsNullOrEmpty(userDto.Phone)) 
        {
            ValidatePhone(userDto.Phone);
            if (userDto.Phone != user.Phone 
                && await this._usersRepository.ExistsAsync(x => x.Phone == userDto.Phone && x.IsDeleted == false, cancellationToken))
            {
                throw new EntityAlreadyExistsException<User>("phone", userDto.Phone);
            }
        }
    }

    private void ValidateEmail(string email)
    {
        string regex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        if (!Regex.IsMatch(email, regex))
        {
            throw new InvalidEmailException(email);
        }
    }

    private void ValidatePhone(string phone)
    {
        string regex = @"^\+[0-9]{1,15}$";

        if (!Regex.IsMatch(phone, regex))
        {
            throw new InvalidPhoneNumberException(phone);
        }
    }
}