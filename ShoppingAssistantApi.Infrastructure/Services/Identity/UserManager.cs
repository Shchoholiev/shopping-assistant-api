using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;
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
public class UserManager : IUserManager
{
    private readonly IUsersRepository _usersRepository;

    private readonly ILogger _logger;

    private readonly IPasswordHasher _passwordHasher;

    private readonly ITokensService _tokensService;

    private readonly IMapper _mapper;

    private readonly IRolesRepository _rolesRepository;

    public UserManager(IUsersRepository usersRepository, ILogger<UserManager> logger, IPasswordHasher passwordHasher, ITokensService tokensService, IMapper mapper, IRolesRepository rolesRepository)
    {
        this._usersRepository = usersRepository;
        this._logger = logger;
        this._passwordHasher = passwordHasher;
        this._tokensService = tokensService;
        this._mapper = mapper;
        this._rolesRepository = rolesRepository;

    }

    public async Task<TokensModel> LoginAsync(AccessUserModel login, CancellationToken cancellationToken)
    {
        var user = login.Email != null
            ? await this._usersRepository.GetUserAsync(x => x.Email == login.Email, cancellationToken)
            : await this._usersRepository.GetUserAsync(x => x.Phone == login.Phone, cancellationToken);

        if (user == null)
        {
            throw new EntityNotFoundException<User>();
        }

        if (!this._passwordHasher.Check(login.Password, user.PasswordHash))
        {
            throw new InvalidDataException("Invalid password!");
        }

        user.RefreshToken = this.GetRefreshToken();
        user.RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(30);
        await this._usersRepository.UpdateUserAsync(user, cancellationToken);
        var tokens = this.GetUserTokens(user);

        this._logger.LogInformation($"Logged in user with email: {login.Email}.");

        return tokens;
    }

    public async Task<TokensModel> AccessGuestAsync(AccessGuestModel guest, CancellationToken cancellationToken)
    {
        var user = await this._usersRepository.GetUserAsync(x => x.GuestId == guest.GuestId, cancellationToken);

        if (user != null)
        {
            user.RefreshToken = this.GetRefreshToken();
            await this._usersRepository.UpdateUserAsync(user, cancellationToken);
            var userTokens = this.GetUserTokens(user);

            this._logger.LogInformation($"Logged in guest with guest id: {guest.GuestId}.");

            return userTokens;
        }

        var role = await this._rolesRepository.GetRoleAsync(r => r.Name == "Guest", cancellationToken);

        var newUser = new User
        {
            GuestId = guest.GuestId,
            Roles = new List<Role> { role },
            RefreshToken = this.GetRefreshToken(),
            RefreshTokenExpiryDate = DateTime.Now.AddDays(30),
            CreatedDateUtc = DateTime.UtcNow,
            LastModifiedDateUtc = DateTime.UtcNow
        };

        await this._usersRepository.AddAsync(newUser, cancellationToken);
        var tokens = this.GetUserTokens(newUser);

        this._logger.LogInformation($"Created guest with guest id: {guest.GuestId}.");

        return tokens;
    }

    public async Task<TokensModel> AddToRoleAsync(string roleName, string id, CancellationToken cancellationToken)
    {
        var role = await this._rolesRepository.GetRoleAsync(r => r.Name == roleName, cancellationToken);
        if (role == null)
        {
            throw new EntityNotFoundException<Role>();
        }

        if (!ObjectId.TryParse(id, out var objectId))
        {
            throw new InvalidDataException("Provided id is invalid.");
        }

        var user = await this._usersRepository.GetUserAsync(objectId, cancellationToken);
        if (user == null)
        {
            throw new EntityNotFoundException<User>();
        }

        user.Roles.Add(role);
        await this._usersRepository.UpdateUserAsync(user, cancellationToken);
        var tokens = this.GetUserTokens(user);

        this._logger.LogInformation($"Added role {roleName} to user with id: {id}.");

        return tokens;
    }

    public async Task<TokensModel> RemoveFromRoleAsync(string roleName, string id, CancellationToken cancellationToken)
    {
        var role = await this._rolesRepository.GetRoleAsync(r => r.Name == roleName, cancellationToken);
        if (role == null)
        {
            throw new EntityNotFoundException<Role>();
        }

        if (!ObjectId.TryParse(id, out var objectId))
        {
            throw new InvalidDataException("Provided id is invalid.");
        }

        var user = await this._usersRepository.GetUserAsync(objectId, cancellationToken);
        if (user == null)
        {
            throw new EntityNotFoundException<User>();
        }

        var deletedRole = user.Roles.Find(x => x.Name == role.Name);

        user.Roles.Remove(deletedRole);
        await this._usersRepository.UpdateUserAsync(user, cancellationToken);
        var tokens = this.GetUserTokens(user);

        this._logger.LogInformation($"Added role {roleName} to user with id: {id}.");

        return tokens;
    }

    public async Task<UpdateUserModel> UpdateAsync(UserDto userDto, CancellationToken cancellationToken)
    {
        if (userDto.Email != null) ValidateEmail(userDto.Email);
        if (userDto.Phone != null) ValidateNumber(userDto.Phone);

        if (userDto.Roles.Any(x => x.Name == "Guest") && !userDto.Roles.Any(x => x.Name == "User"))
        {
            if (userDto.Password != null && (userDto.Email != null || userDto.Phone != null))
            {
                var roleEntity = await this._rolesRepository.GetRoleAsync(x => x.Name == "User", cancellationToken);
                var roleDto = this._mapper.Map<RoleDto>(roleEntity);
                userDto.Roles.Add(roleDto);
            }
        }

        var user = await this._usersRepository.GetUserAsync(x => x.Id == GlobalUser.Id, cancellationToken);

        if (user == null)
        {
            throw new EntityNotFoundException<User>();
        }

        if (userDto.Roles.Any(x => x.Name == "User") && userDto.Email != null)
        {
            if (await this._usersRepository.GetUserAsync(x => x.Email == userDto.Email, cancellationToken) != null)
            {
                throw new EntityAlreadyExistsException<User>("email", userDto.Email);
            }
        }
        if (userDto.Roles.Any(x => x.Name == "User") && userDto.Phone != null)
        {
            if (await this._usersRepository.GetUserAsync(x => x.Phone == userDto.Phone, cancellationToken) != null)
            {
                throw new EntityAlreadyExistsException<User>("phone", userDto.Phone);
            }
        }

        this._mapper.Map(userDto, user);
        if (!userDto.Password.IsNullOrEmpty())
        {
            user.PasswordHash = this._passwordHasher.Hash(userDto.Password);
        }
        user.RefreshToken = this.GetRefreshToken();
        await this._usersRepository.UpdateUserAsync(user, cancellationToken);

        var tokens = this.GetUserTokens(user);

        this._logger.LogInformation($"Update user with id: {GlobalUser.Id.ToString()}.");

        return new UpdateUserModel() { Tokens = tokens, User = this._mapper.Map<UserDto>(user) };
    }

    public async Task<UpdateUserModel> UpdateUserByAdminAsync(string id, UserDto userDto, CancellationToken cancellationToken)
    {
        if (!ObjectId.TryParse(id, out var objectId))
        {
            throw new InvalidDataException("Provided id is invalid.");
        }

        var user = await this._usersRepository.GetUserAsync(objectId, cancellationToken);

        if (user == null)
        {
            throw new EntityNotFoundException<User>();
        }

        this._mapper.Map(userDto, user);

        user.RefreshToken = this.GetRefreshToken();
        await this._usersRepository.UpdateUserAsync(user, cancellationToken);

        var tokens = this.GetUserTokens(user);

        this._logger.LogInformation($"Update user with id: {id}.");

        return new UpdateUserModel() { Tokens = tokens, User = this._mapper.Map<UserDto>(user) };
    }

    private string GetRefreshToken()
    {
        var refreshToken = this._tokensService.GenerateRefreshToken();

        this._logger.LogInformation($"Returned new refresh token.");

        return refreshToken;
    }

    private TokensModel GetUserTokens(User user)
    {
        var claims = this.GetClaims(user);
        var accessToken = this._tokensService.GenerateAccessToken(claims);

        this._logger.LogInformation($"Returned new access and refresh tokens.");

        return new TokensModel
        {
            AccessToken = accessToken,
            RefreshToken = user.RefreshToken,
        };
    }

    private IEnumerable<Claim> GetClaims(User user)
    {
        var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.MobilePhone, user.Phone ?? string.Empty),
            };

        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Name));
        }

        this._logger.LogInformation($"Returned claims for user with id: {user.Id.ToString()}.");

        return claims;
    }

    private void ValidateEmail(string email)
    {
        string regex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        if (!Regex.IsMatch(email, regex))
        {
            throw new InvalidEmailException(email);
        }
    }

    private void ValidateNumber(string phone)
    {
        string regex = @"^\+[0-9]{1,15}$";

        if (!Regex.IsMatch(phone, regex))
        {
            throw new InvalidPhoneNumberException(phone);
        }
    }
}