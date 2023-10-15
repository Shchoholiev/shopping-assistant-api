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
        var products = new Product[]
        {
            new Product()
            {
                Name = "Thermaltake Glacier 360 Liquid-Cooled PC",
                Description = "Cool PC for any task!",
                Rating = 4.3,
                Url = "https://www.amazon.com/Thermaltake-Liquid-Cooled-ToughRAM-Computer-S3WT-B550-G36-LCS/dp" +
                      "/B09FYNM2GW/ref=sr_1_1?crid=391KAS4JFJSFF&keywords=gaming%2Bpc&qid=1697132083&sprefix=gaming%2Bpc%2Caps%2C209&sr=8-1&th=1",
                ImagesUrls = new string[]
                {
                    "https://m.media-amazon.com/images/I/61cXu9yGldL._AC_SL1200_.jpg",
                    "https://m.media-amazon.com/images/I/615gxSGp42L._AC_SL1200_.jpg"
                },
                CreatedDateUtc = DateTime.UtcNow
            }, 
            
            new Product()
            {
                Name = "Apple MagSafe Battery Pack",
                Description = "Portable Charger with Fast Charging Capability, Power Bank Compatible with iPhone",
                Rating = 4.3,
                Url = "https://www.amazon.com/Apple-MJWY3AM-A-MagSafe-Battery/dp/" +
                      "B099BWY7WT/ref=sr_1_1?keywords=apple+power+bank&qid=1697375350&sr=8-1",
                ImagesUrls = new string[]
                {
                    "https://m.media-amazon.com/images/I/418SjFMB1wL._AC_SX679_.jpg",
                    "https://m.media-amazon.com/images/I/51v4pgChtLL._AC_SX679_.jpg",
                    "https://m.media-amazon.com/images/I/61mJ0z7uYQL._AC_SX679_.jpg"
                },
                CreatedDateUtc = DateTime.UtcNow
            },
            
            new Product()
            {
                Name = "Logitech K400 Plus Wireless Touch With Easy Media Control and Built-in Touchpad",
                Description = "Reliable membrane keyboard with touchpad!",
                Rating = 4.5,
                Url = "https://www.amazon.com/Logitech-Wireless-Keyboard-Touchpad-PC-connected/dp/B014EUQOGK/" +
                      "ref=sr_1_11?crid=BU2PHZKHKD65&keywords=keyboard+wireless&qid=1697375559&sprefix=keyboard+wir%2Caps%2C195&sr=8-11",
                ImagesUrls = new string[]
                {
                    "https://m.media-amazon.com/images/I/51yjnWJ5urL._AC_SX466_.jpg",
                    "https://m.media-amazon.com/images/I/71al70zP7QL._AC_SX466_.jpg",
                    "https://m.media-amazon.com/images/I/71+JXDDY01L._AC_SX466_.jpg"
                },
                CreatedDateUtc = DateTime.UtcNow
            },
            
            new Product()
            {
                Name = "Logitech MX Anywhere 2S Wireless Mouse Use On Any Surface",
                Description = "Cross computer control: Game changing capacity to navigate seamlessly on three computers," +
                              " and copy paste text, images, and files from one to the other using Logitech Flow",
                Rating = 4.6,
                Url = "https://www.amazon.com/Logitech-Hyper-Fast-Scrolling-Rechargeable-Computers/dp/B08P2JFPQC/ref=sr_1_8?" +
                      "crid=2BL6Z14W2TPP3&keywords=mouse%2Bwireless&qid=1697375784&sprefix=mousewireless%2Caps%2C197&sr=8-8&th=1",
                ImagesUrls = new string[]
                {
                    "https://m.media-amazon.com/images/I/6170mJHIsYL._AC_SX466_.jpg",
                    "https://m.media-amazon.com/images/I/71a5As76MDL._AC_SX466_.jpg"
                },
                CreatedDateUtc = DateTime.UtcNow
            }
        };

        await _productCollection.InsertManyAsync(products);
    }
}
