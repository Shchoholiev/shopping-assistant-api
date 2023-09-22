﻿using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using ShoppingAssistantApi.Application.GlobalInstances;
using ShoppingAssistantApi.Application.IServices;
using ShoppingAssistantApi.Application.IServices.Identity;
using ShoppingAssistantApi.Application.Models.CreateDtos;
using ShoppingAssistantApi.Application.Models.Dtos;
using ShoppingAssistantApi.Application.Models.Identity;

namespace ShoppingAssistantApi.Persistance.PersistanceExtentions;

public class DbInitialaizer
{
    private readonly IUsersService _usersService;

    private readonly IUserManager _userManager;

    private readonly IRolesService _rolesService;

    private readonly ITokensService _tokensService;


    public IEnumerable<RoleDto> Roles { get; set; }

    public DbInitialaizer(IServiceProvider serviceProvider)
    {
        this._usersService = serviceProvider.GetService<IUsersService>();
        this._rolesService = serviceProvider.GetService<IRolesService>();
        this._userManager = serviceProvider.GetService<IUserManager>();
        this._tokensService = serviceProvider.GetService<ITokensService>();
    }

    public async 
    Task
InitialaizeDb(CancellationToken cancellationToken)
    {
        await this.AddRoles(cancellationToken);
        await this.AddUsers(cancellationToken);
    }

    public async Task AddUsers(CancellationToken cancellationToken)
    {
        var guestModel1 = new AccessGuestModel
        {
            GuestId = Guid.NewGuid(),
        };

        var guestModel2 = new AccessGuestModel
        {
            GuestId = Guid.NewGuid(),
        };

        var guestModel3 = new AccessGuestModel
        {
            GuestId = Guid.NewGuid(),
        };

        var guestModel4 = new AccessGuestModel
        {
            GuestId = Guid.NewGuid(),
        };

        var guestModel5 = new AccessGuestModel
        {
            GuestId = Guid.NewGuid(),
        };

        Task.WaitAll(
            _userManager.AccessGuestAsync(guestModel1, cancellationToken),
            _userManager.AccessGuestAsync(guestModel2, cancellationToken),
            _userManager.AccessGuestAsync(guestModel3, cancellationToken),
            _userManager.AccessGuestAsync(guestModel4, cancellationToken),
            _userManager.AccessGuestAsync(guestModel5, cancellationToken)
        );

        var guests = await this._usersService.GetUsersPageAsync(1, 4, cancellationToken);
        var guestsResult = guests.Items.ToList();

        var user1 = new UserDto
        {
            Id = guestsResult[0].Id,
            GuestId = guestsResult[0].GuestId,
            Roles = guestsResult[0].Roles,
            Phone = "+380953326869",
            Email = "mykhailo.bilodid@nure.ua",
            Password = "Yuiop12345",
            RefreshToken = _tokensService.GenerateRefreshToken(),
            RefreshTokenExpiryDate = DateTime.Now.AddDays(7),
        };

        var user2 = new UserDto
        {
            Id = guestsResult[1].Id,
            GuestId = guestsResult[1].GuestId,
            Roles = guestsResult[1].Roles,
            Phone = "+380953326888",
            Email = "serhii.shchoholiev@nure.ua",
            Password = "Yuiop12345",
            RefreshToken = _tokensService.GenerateRefreshToken(),
            RefreshTokenExpiryDate = DateTime.Now.AddDays(7),
        };

        var user3 = new UserDto
        {
            Id = guestsResult[2].Id,
            GuestId = guestsResult[2].GuestId,
            Roles = guestsResult[2].Roles,
            Phone = "+380983326869",
            Email = "vitalii.krasnorutski@nure.ua",
            Password = "Yuiop12345",
            RefreshToken = _tokensService.GenerateRefreshToken(),
            RefreshTokenExpiryDate = DateTime.Now.AddDays(7),
        };

        var user4 = new UserDto
        {
            Id = guestsResult[3].Id,
            GuestId = guestsResult[3].GuestId,
            Roles = guestsResult[3].Roles,
            Phone = "+380953826869",
            Email = "shopping.assistant.team@gmail.com",
            Password = "Yuiop12345",
            RefreshToken = _tokensService.GenerateRefreshToken(),
            RefreshTokenExpiryDate = DateTime.Now.AddDays(7),
        };
        
        GlobalUser.Id = ObjectId.Parse(user1.Id);
        await _userManager.UpdateAsync(user1, cancellationToken);

        GlobalUser.Id = ObjectId.Parse(user2.Id);
        await _userManager.UpdateAsync(user2, cancellationToken);

        GlobalUser.Id = ObjectId.Parse(user3.Id);
        await _userManager.UpdateAsync(user3, cancellationToken);

        GlobalUser.Id = ObjectId.Parse(user4.Id);
        await _userManager.UpdateAsync(user4, cancellationToken);
    }

    public async Task AddRoles(CancellationToken cancellationToken)
    {
        var role1 = new RoleCreateDto
        {
            Name = "User"
        };

        var role2 = new RoleCreateDto
        {
            Name = "Admin"
        };

        var role3 = new RoleCreateDto
        {
            Name = "Guest"
        };

        var dto1 = await _rolesService.AddRoleAsync(role1, cancellationToken);
        var dto2 = await _rolesService.AddRoleAsync(role2, cancellationToken);
        var dto3 = await _rolesService.AddRoleAsync(role3, cancellationToken);
    }
}
