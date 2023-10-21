using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using ShoppingAssistantApi.Application.IServices.Identity;
using ShoppingAssistantApi.Domain.Entities;
using ShoppingAssistantApi.Domain.Enums;
using ShoppingAssistantApi.Infrastructure.Services.Identity;
using ShoppingAssistantApi.Persistance.Database;

namespace ShoppingAssistantApi.Persistance.PersistanceExtentions;

public class DbInitialaizer
{
    private readonly IMongoCollection<User> _userCollection;

    private readonly IMongoCollection<Role> _roleCollection;

    private readonly IMongoCollection<Wishlist> _wishlistCollection;

    private readonly IMongoCollection<Product> _productCollection;

    private readonly PasswordHasher passwordHasher;

    public DbInitialaizer(IServiceProvider serviceProvider)
    {
        passwordHasher = new PasswordHasher(new Logger<PasswordHasher>(new LoggerFactory()));
        _wishlistCollection = serviceProvider.GetService<MongoDbContext>().Db.GetCollection<Wishlist>("Wishlists");
        _userCollection = serviceProvider.GetService<MongoDbContext>().Db.GetCollection<User>("Users");
        _roleCollection = serviceProvider.GetService<MongoDbContext>().Db.GetCollection<Role>("Roles");
        _productCollection = serviceProvider.GetService<MongoDbContext>().Db.GetCollection<Product>("Product");
    }

    public async Task InitialaizeDb(CancellationToken cancellationToken)
    {
        await AddRoles(cancellationToken);
        await AddUsers(cancellationToken);
        await AddWishlistsWithMessages(cancellationToken);
        await AddProducts(cancellationToken);
    }

    public async Task AddUsers(CancellationToken cancellationToken)
    {
        var userRole = await (await _roleCollection.FindAsync(x => x.Name.Equals("User"))).FirstAsync();
        var guestRole = await (await _roleCollection.FindAsync(x => x.Name.Equals("Guest"))).FirstAsync();
        var adminRole = await (await _roleCollection.FindAsync(x => x.Name.Equals("Admin"))).FirstAsync();

        var users = new User[]
        {
            new User
            {
                Id = ObjectId.Parse("6533bb29c8c22b038c71cf46"),
                GuestId = Guid.NewGuid(),
                Roles = new List<Role> {guestRole},
                CreatedById = ObjectId.Parse("6533bb29c8c22b038c71cf46"),
                CreatedDateUtc = DateTime.UtcNow,
                LastModifiedById = ObjectId.Parse("6533bb29c8c22b038c71cf46"),
                LastModifiedDateUtc = DateTime.UtcNow,
                IsDeleted = false
            },

            new User
            {
                Id = ObjectId.Parse("6533bde5755745116be42ce7"),
                GuestId = Guid.NewGuid(),
                Roles = new List<Role>
                {
                    guestRole,
                    userRole
                },
                Phone = "+380953326869",
                Email = "mykhailo.bilodid@nure.ua",
                PasswordHash = this.passwordHasher.Hash("Yuiop12345"),
                CreatedById = ObjectId.Parse("6533bde5755745116be42ce7"),
                CreatedDateUtc = DateTime.UtcNow,
                LastModifiedById = ObjectId.Parse("6533bde5755745116be42ce7"),
                LastModifiedDateUtc = DateTime.UtcNow,
                IsDeleted = false
            },

            new User
            {
                Id = ObjectId.Parse("6533bded80fbc6e96250575b"),
                GuestId = Guid.NewGuid(),
                Roles = new List<Role>
                {
                    guestRole,
                    userRole,
                    adminRole
                },
                Phone = "+380953826869",
                Email = "shopping.assistant.team@gmail.com",
                PasswordHash = this.passwordHasher.Hash("Yuiop12345"),
                CreatedById = ObjectId.Parse("6533bded80fbc6e96250575b"),
                CreatedDateUtc = DateTime.UtcNow,
                LastModifiedById = ObjectId.Parse("6533bded80fbc6e96250575b"),
                LastModifiedDateUtc = DateTime.UtcNow,
                IsDeleted = false            },

            new User
            {
                Id = ObjectId.Parse("6533bdf9efaca5bb0894f992"),
                GuestId = Guid.NewGuid(),
                Roles = new List<Role>
                {
                    guestRole,
                    userRole
                },
                Phone = "+380983326869",
                Email = "vitalii.krasnorutski@nure.ua",
                PasswordHash = this.passwordHasher.Hash("Yuiop12345"),
                CreatedById = ObjectId.Parse("6533bdf9efaca5bb0894f992"),
                CreatedDateUtc = DateTime.UtcNow,
                LastModifiedById = ObjectId.Parse("6533bdf9efaca5bb0894f992"),
                LastModifiedDateUtc = DateTime.UtcNow,
                IsDeleted = false            },

            new User
            {
                Id = ObjectId.Parse("6533be06d1b78a76c664ddae"),
                GuestId = Guid.NewGuid(),
                Roles = new List<Role>
                {
                    guestRole,
                    userRole
                },
                Phone = "+380953326888",
                Email = "serhii.shchoholiev@nure.ua",
                PasswordHash = this.passwordHasher.Hash("Yuiop12345"),
                CreatedById = ObjectId.Parse("6533be06d1b78a76c664ddae"),
                CreatedDateUtc = DateTime.UtcNow,
                LastModifiedById = ObjectId.Parse("6533be06d1b78a76c664ddae"),
                LastModifiedDateUtc = DateTime.UtcNow,
                IsDeleted = false            }
        };

        await _userCollection.InsertManyAsync(users);
    }

    public async Task AddRoles(CancellationToken cancellationToken)
    {
        var roles = new Role[]
        {
            new Role
            {
                Id = ObjectId.Parse("6533b5882e7867b8b21e7b27"),
                Name = "User",
                CreatedById = ObjectId.Parse("6533bded80fbc6e96250575b"),
                CreatedDateUtc = DateTime.UtcNow,
                LastModifiedById = ObjectId.Parse("6533bded80fbc6e96250575b"),
                LastModifiedDateUtc = DateTime.UtcNow,
                IsDeleted = false
            },

            new Role
            {
                Id = ObjectId.Parse("6533b591a7f31776cd2d50fc"),
                Name = "Guest",
                CreatedById = ObjectId.Parse("6533bded80fbc6e96250575b"),
                CreatedDateUtc = DateTime.UtcNow,
                LastModifiedById = ObjectId.Parse("6533bded80fbc6e96250575b"),
                LastModifiedDateUtc = DateTime.UtcNow,
                IsDeleted = false
            },

            new Role
            {
                Id = ObjectId.Parse("6533b59d1b09ab2618af5ff3"),
                Name = "Admin",
                CreatedById = ObjectId.Parse("6533bded80fbc6e96250575b"),
                CreatedDateUtc = DateTime.UtcNow,
                LastModifiedById = ObjectId.Parse("6533bded80fbc6e96250575b"),
                LastModifiedDateUtc = DateTime.UtcNow,
                IsDeleted = false
            },
        };
        await _roleCollection.InsertManyAsync(roles);
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
                        WishlistId = ObjectId.Parse("ab79cde6f69abcd3efab65cd"),
                        CreatedById = user1.Id,
                        CreatedDateUtc = DateTime.UtcNow
                    },
                    new Message
                    {
                        Text = "Answer",
                        Role = MessageRoles.Application.ToString(),
                        WishlistId = ObjectId.Parse("ab79cde6f69abcd3efab65cd"),
                        CreatedById = user1.Id,
                        CreatedDateUtc = DateTime.UtcNow
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
                        WishlistId = ObjectId.Parse("ab6c2c2d9edf39abcd1ef9ab"),
                        CreatedById = user1.Id,
                        CreatedDateUtc = DateTime.UtcNow
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
                CreatedDateUtc = DateTime.UtcNow,
                WasOpened = false,
                WishlistId = ObjectId.Parse("ab79cde6f69abcd3efab65cd")
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
                CreatedDateUtc = DateTime.UtcNow,
                WasOpened = false,
                WishlistId = ObjectId.Parse("ab79cde6f69abcd3efab65cd")
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
                CreatedDateUtc = DateTime.UtcNow,
                WasOpened = false,
                WishlistId = ObjectId.Parse("ab6c2c2d9edf39abcd1ef9ab")
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
                CreatedDateUtc = DateTime.UtcNow,
                WasOpened = false,
                WishlistId = ObjectId.Parse("ab6c2c2d9edf39abcd1ef9ab")
            }
        };

        await _productCollection.InsertManyAsync(products);
    }
}
