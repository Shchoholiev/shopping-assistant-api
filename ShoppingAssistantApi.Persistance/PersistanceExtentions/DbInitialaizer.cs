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

    private readonly IMongoCollection<User> _userCollection;

    private readonly IMongoCollection<Wishlist> _wishlistCollection;

    private readonly IMongoCollection<Message> _messageCollection;

    private readonly IMongoCollection<Product> _productCollection;

    public IEnumerable<RoleDto> Roles { get; set; }

    public DbInitialaizer(IServiceProvider serviceProvider)
    {
        _usersService = serviceProvider.GetService<IUsersService>();
        _rolesService = serviceProvider.GetService<IRolesService>();
        _userManager = serviceProvider.GetService<IUserManager>();
        _tokensService = serviceProvider.GetService<ITokensService>();
        _userCollection = serviceProvider.GetService<MongoDbContext>().Db.GetCollection<User>("Users");
        _wishlistCollection = serviceProvider.GetService<MongoDbContext>().Db.GetCollection<Wishlist>("Wishlists");
        _messageCollection = serviceProvider.GetService<MongoDbContext>().Db.GetCollection<Message>("Messages");
        _productCollection = serviceProvider.GetService<MongoDbContext>().Db.GetCollection<Product>("Products");
    }

    public async Task InitialaizeDb(CancellationToken cancellationToken)
    {
        await AddRoles(cancellationToken);
        await AddUsers(cancellationToken);
        await AddWishlistsWithMessagesAndProducts(cancellationToken);
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

    public async Task AddWishlistsWithMessagesAndProducts(CancellationToken cancellationToken)
    {
        var user1 = await (await _userCollection.FindAsync(x => x.Email.Equals("shopping.assistant.team@gmail.com"))).FirstAsync();
        var user2 = await (await _userCollection.FindAsync(x => x.Email.Equals("mykhailo.bilodid@nure.ua"))).FirstAsync();

        var wishlistId1 = ObjectId.Parse("ab79cde6f69abcd3efab65cd");
        var wishlistId2 = ObjectId.Parse("ab6c2c2d9edf39abcd1ef9ab");

        var wishlists = new Wishlist[]
        {
            new Wishlist
            {
                Id = wishlistId1,
                Name = "Gaming PC",
                Type = WishlistTypes.Product.ToString(),
                CreatedById = user1.Id,
                CreatedDateUtc = DateTime.UtcNow
            },
            new Wishlist
            {
                Id = wishlistId2,
                Name = "Generic Wishlist Name",
                Type = WishlistTypes.Product.ToString(),
                CreatedById = user2.Id,
                CreatedDateUtc = DateTime.UtcNow
            }
        };

        await _wishlistCollection.InsertManyAsync(wishlists);

        var messages = new Message[]
        {
            new Message
            {
                Text = "Message 1",
                Role = MessageRoles.User.ToString(),
                WishlistId = wishlistId1,
                CreatedById = user1.Id,
                CreatedDateUtc = DateTime.UtcNow
            },
            new Message
            {
                Text = "Message 2",
                Role = MessageRoles.Application.ToString(),
                WishlistId = wishlistId1,
                CreatedDateUtc = DateTime.UtcNow.AddSeconds(5)
            },
            new Message
            {
                Text = "Message 3",
                Role = MessageRoles.User.ToString(),
                WishlistId = wishlistId1,
                CreatedById = user1.Id,
                CreatedDateUtc = DateTime.UtcNow.AddSeconds(20)
            },
            new Message
            {
                Text = "Message 4",
                Role = MessageRoles.Application.ToString(),
                WishlistId = wishlistId1,
                CreatedDateUtc = DateTime.UtcNow.AddSeconds(25)
            },
            new Message
            {
                Text = "Message 5",
                Role = MessageRoles.User.ToString(),
                WishlistId = wishlistId1,
                CreatedById = user1.Id,
                CreatedDateUtc = DateTime.UtcNow.AddSeconds(45)
            },
            new Message
            {
                Text = "Message 6",
                Role = MessageRoles.Application.ToString(),
                WishlistId = wishlistId1,
                CreatedDateUtc = DateTime.UtcNow.AddSeconds(50)
            },
            new Message
            {
                Text = "Prompt",
                Role = MessageRoles.User.ToString(),
                WishlistId = wishlistId2,
                CreatedById = user2.Id,
                CreatedDateUtc = DateTime.UtcNow
            }
        };

        await _messageCollection.InsertManyAsync(messages);

        var products = new Product[]
        {
            new Product
            {
                Name = "AMD Ryzen 5 5600G 6-Core 12-Thread Unlocked Desktop Processor with Radeon Graphics",
                Description = "Features best-in-class graphics performance in a desktop processor for smooth 1080p gaming, no graphics card required",
                Rating = 4.8,
                Url = "https://a.co/d/5ceuIrq",
                ImagesUrls = new string[]
                {
                    "https://m.media-amazon.com/images/I/51f2hkWjTlL._AC_SL1200_.jpg",
                    "https://m.media-amazon.com/images/I/51iji7Gel-L._AC_SL1200_.jpg"
                },
                WasOpened = false,
                WishlistId = wishlistId1,
                CreatedById = user1.Id,
                CreatedDateUtc = DateTime.UtcNow
            },
            new Product
            {
                Name = "Samsung 970 EVO Plus SSD 2TB NVMe M.2 Internal Solid State Hard Drive, V-NAND Technology, Storage and Memory Expansion for Gaming, Graphics w/ Heat Control, Max Speed, MZ-V7S2T0B/AM ",
                Description = "7 Year Limited Warranty: The 970 EVO Plus provides up to 1200 TBW (Terabytes Written) with 5-years of protection for exceptional endurance powered by the latest V-NAND technology and Samsung's reputation for quality ",
                Rating = 4.8,
                Url = "https://a.co/d/gxnuqs1",
                ImagesUrls = new string[]
                {
                    "https://m.media-amazon.com/images/I/51Brl+iYtvL._AC_SL1001_.jpg",
                    "https://m.media-amazon.com/images/I/51GOfLlVwoL._AC_SL1001_.jpg"
                },
                WasOpened = false,
                WishlistId = wishlistId1,
                CreatedById = user1.Id,
                CreatedDateUtc = DateTime.UtcNow
            },
        };

        await _productCollection.InsertManyAsync(products);
    }
}
