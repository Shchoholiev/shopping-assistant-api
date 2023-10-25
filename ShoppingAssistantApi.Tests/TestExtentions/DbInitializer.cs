using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using ShoppingAssistantApi.Domain.Entities;
using ShoppingAssistantApi.Domain.Enums;
using ShoppingAssistantApi.Infrastructure.Services.Identity;
using ShoppingAssistantApi.Persistance.Database;

namespace ShoppingAssistantApi.Tests.TestExtentions;

public class DbInitializer
{
    private readonly MongoDbContext _dbContext;

    public DbInitializer(MongoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void InitializeDb()
    {
        _dbContext.Client.DropDatabase(_dbContext.Db.DatabaseNamespace.DatabaseName);
        
        InitializeUsersAsync().Wait();
        InitializeWishlistsAsync().Wait();
        InitializeMessagesAsync().Wait();
        InitializeProductsAsync().Wait();
    }

    public async Task InitializeUsersAsync()
    {
        #region Roles

        var rolesCollection = _dbContext.Db.GetCollection<Role>("Roles");

        var questRole = new Role
        {
            Name = "Guest"
        };
        await rolesCollection.InsertOneAsync(questRole);

        var userRole = new Role
        {
            Name = "User"
        };
        await rolesCollection.InsertOneAsync(userRole);

        var adminRole = new Role
        {
            Name = "Admin"
        };
        await rolesCollection.InsertOneAsync(adminRole);

        #endregion

        #region Users

        var passwordHasher = new PasswordHasher(new Logger<PasswordHasher>(new LoggerFactory()));
        var usersCollection = _dbContext.Db.GetCollection<User>("Users");

        var testUser = new User
        {
            Id = ObjectId.Parse("652c3b89ae02a3135d6409fc"),
            Email = "test@gmail.com",
            Phone = "+380953326869",
            Roles = new List<Role> { questRole, userRole },
            PasswordHash = passwordHasher.Hash("Yuiop12345"),
            CreatedById = ObjectId.Empty,
            CreatedDateUtc = DateTime.UtcNow
        };
        await usersCollection.InsertOneAsync(testUser);

        var adminUser = new User
        {
            Id = ObjectId.Parse("652c3b89ae02a3135d6408fc"),
            Email = "admin@gmail.com",
            Phone = "+12345678901",
            Roles = new List<Role> { questRole, userRole, adminRole },
            PasswordHash = passwordHasher.Hash("Yuiop12345"),
            CreatedById = ObjectId.Empty,
            CreatedDateUtc = DateTime.UtcNow
        };
        await usersCollection.InsertOneAsync(adminUser);

        var wishlistsUser = new User
        {
            Id = ObjectId.Parse("652c3b89ae02a3135d6418fc"),
            Email = "wishlists@gmail.com",
            Phone = "+12234567890",
            Roles = new List<Role> { questRole },
            PasswordHash = passwordHasher.Hash("Yuiop12345"),
            CreatedById = ObjectId.Empty,
            CreatedDateUtc = DateTime.UtcNow
        };
        await usersCollection.InsertOneAsync(wishlistsUser);

        #endregion
    }

    public async Task InitializeWishlistsAsync()
    {
        var wishlistsCollection = _dbContext.Db.GetCollection<Wishlist>("Wishlists");
        var usersCollection = _dbContext.Db.GetCollection<User>("Users");

        var user1 = await (await usersCollection.FindAsync(x => x.Email!.Equals("wishlists@gmail.com"))).FirstAsync();
        var user2 = await (await usersCollection.FindAsync(x => x.Email!.Equals("test@gmail.com"))).FirstAsync();

        var wishlistId1 = ObjectId.Parse("ab79cde6f69abcd3efab65cd");
        var wishlistId2 = ObjectId.Parse("ab6c2c2d9edf39abcd1ef9ab");
        var wishlistId3 = ObjectId.Parse("ab7c8c2d9edf39abcd1ef9ab");
        var wishlistId4 = ObjectId.Parse("ab8c8c2d9edf39abcd1ef9ab");

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
            },
            new Wishlist
            {
                Id = wishlistId3,
                Name = "Test For Search",
                Type = WishlistTypes.Product.ToString(),
                CreatedById = user1.Id,
                CreatedDateUtc = DateTime.UtcNow
            },
            new Wishlist
            {
                Id = wishlistId4,
                Name = "Test For Answer",
                Type = WishlistTypes.Product.ToString(),
                CreatedById = user1.Id,
                CreatedDateUtc = DateTime.UtcNow
            },
        };

        await wishlistsCollection.InsertManyAsync(wishlists);
    }


    public async Task InitializeMessagesAsync()
    {
        var messagesCollection = _dbContext.Db.GetCollection<Message>("Messages");
        var usersCollection = _dbContext.Db.GetCollection<User>("Users");

        var user1 = await (await usersCollection.FindAsync(x => x.Email!.Equals("wishlists@gmail.com"))).FirstAsync();
        var user2 = await (await usersCollection.FindAsync(x => x.Email!.Equals("test@gmail.com"))).FirstAsync();

        var wishlistId1 = ObjectId.Parse("ab79cde6f69abcd3efab65cd");
        var wishlistId2 = ObjectId.Parse("ab6c2c2d9edf39abcd1ef9ab");
        var wishlistId3 = ObjectId.Parse("ab7c8c2d9edf39abcd1ef9ab");
        var wishlistId4 = ObjectId.Parse("ab8c8c2d9edf39abcd1ef9ab");

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
            },
            new Message
            {
                Text = "You are a Shopping Assistant that helps people find product recommendations. Ask user additional questions if more context needed." +
                       "\nYou must return data with one of the prefixes:" +
                       "\n[Question] - return question" +
                       "\n[Suggestions] - return semicolon separated suggestion how to answer to a question" +
                       "\n[Message] - return text" +
                       "\n[Products] - return semicolon separated product names",
                Role = "system",
                WishlistId = wishlistId4,
                CreatedById = user2.Id,
                CreatedDateUtc = DateTime.UtcNow
            },
            new Message
            {
                Text = "What are you looking for?",
                Role = "system",
                WishlistId = wishlistId4,
                CreatedById = user2.Id,
                CreatedDateUtc = DateTime.UtcNow
            },
        };

        await messagesCollection.InsertManyAsync(messages);
    }

    public async Task InitializeProductsAsync()
    {
        var productsCollection = _dbContext.Db.GetCollection<Product>("Products");
        var usersCollection = _dbContext.Db.GetCollection<User>("Users");

        var user1 = await (await usersCollection.FindAsync(x => x.Email!.Equals("wishlists@gmail.com"))).FirstAsync();
        var user2 = await (await usersCollection.FindAsync(x => x.Email!.Equals("test@gmail.com"))).FirstAsync();

        var wishlistId1 = ObjectId.Parse("ab79cde6f69abcd3efab65cd");
        var wishlistId2 = ObjectId.Parse("ab6c2c2d9edf39abcd1ef9ab");

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

        await productsCollection.InsertManyAsync(products);
    }
}
