using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using ShoppingAssistantApi.Application.GlobalInstances;
using ShoppingAssistantApi.Application.IServices;
using ShoppingAssistantApi.Application.IServices.Identity;
using ShoppingAssistantApi.Application.Models.CreateDtos;
using ShoppingAssistantApi.Application.Models.Dtos;
using ShoppingAssistantApi.Application.Models.Identity;
using ShoppingAssistantApi.Domain.Entities;
using ShoppingAssistantApi.Domain.Enums;
using ShoppingAssistantApi.Persistance.Database;

namespace ShoppingAssistantApi.Persistance.PersistanceExtentions;

public class DbInitialaizer
{
    private readonly IUsersService _usersService;

    private readonly IUserManager _userManager;

    private readonly IRolesService _rolesService;

    private readonly ITokensService _tokensService;

    private readonly IWishlistsService _wishlistsService;

    private readonly IMongoCollection<User> _userCollection;

    private readonly IMongoCollection<Wishlist> _wishlistCollection;
    
    private readonly IMongoCollection<Product> _productCollection;

    public IEnumerable<RoleDto> Roles { get; set; }

    public DbInitialaizer(IServiceProvider serviceProvider)
    {
        _usersService = serviceProvider.GetService<IUsersService>();
        _rolesService = serviceProvider.GetService<IRolesService>();
        _userManager = serviceProvider.GetService<IUserManager>();
        _tokensService = serviceProvider.GetService<ITokensService>();
        _wishlistsService = serviceProvider.GetService<IWishlistsService>();
        _wishlistCollection = serviceProvider.GetService<MongoDbContext>().Db.GetCollection<Wishlist>("Wishlists");
        _userCollection = serviceProvider.GetService<MongoDbContext>().Db.GetCollection<User>("Users");
        _productCollection = serviceProvider.GetService<MongoDbContext>().Db.GetCollection<Product>("Product");
    }

    public async Task InitialaizeDb(CancellationToken cancellationToken)
    {
        await AddRoles(cancellationToken);
        await AddUsers(cancellationToken);
        await AddWishlistsWithMessages(cancellationToken);
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

        var guests = await _usersService.GetUsersPageAsync(1, 4, cancellationToken);
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

    public async Task AddWishlistsWithMessages(CancellationToken cancellationToken)
    {
        var user1 = await (await _userCollection.FindAsync(x => x.Email.Equals("shopping.assistant.team@gmail.com"))).FirstAsync();
        var user2 = await (await _userCollection.FindAsync(x => x.Email.Equals("mykhailo.bilodid@nure.ua"))).FirstAsync();

        var wishlists = new Wishlist[]
        {
            new Wishlist
            {
                Id = ObjectId.Parse("ab79cde6f69abcd3efab65cd"),
                Name = "Gaming PC",
                Type = WishlistTypes.Product.ToString(),
                CreatedById = user1.Id,
                Messages = new Message[]
                {
                    new Message
                    {
                        Text = "Prompt",
                        Role = MessageRoles.User.ToString(),
                    },
                    new Message
                    {
                        Text = "Answer",
                        Role = MessageRoles.Application.ToString(),
                    },
                }
            },
            new Wishlist
            {
                Id = ObjectId.Parse("ab6c2c2d9edf39abcd1ef9ab"),
                Name = "Generic Wishlist Name",
                Type = WishlistTypes.Product.ToString(),
                CreatedById = user2.Id,
                Messages = new Message[]
                {
                    new Message
                    {
                        Text = "Prompt",
                        Role = MessageRoles.User.ToString(),
                    }
                }
            }
        };

        await _wishlistCollection.InsertManyAsync(wishlists);
    }

    public async Task AddProducts(CancellationToken cancellationToken)
    {
        var wishList1 = await _wishlistCollection.FindAsync(w => w.Name == "Gaming PC");
        var wishList2 = await _wishlistCollection.FindAsync(w => w.Name == "Generic Wishlist Name");

        var products = new Product[]
        {
            new Product()
            {
                Id = ObjectId.Parse("ab6c2c2d9edf39abcd1ef9ab"),
                Url = "url",
                Name = "Thermaltake Glacier",
                Description = "Something",
                Rating = 4.1,
                ImagesUrls = new string[]
                {
                    "url1",
                    "url2"
                }
            }, 
            
            new Product()
            {
                Id = ObjectId.Parse("ab6c2c2d9edf39abcd1ef9ab"),
                Url = "url",
                Name = "Mac",
                Description = "very very cool laptop",
                Rating = 4.9,
                ImagesUrls = new string[]
                {
                    "url1",
                    "url2"
                }
            }
            
        };
        
    }
}
