using Microsoft.Extensions.Logging;
using MongoDB.Bson;
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

        var gamingPcWishlist = new Wishlist
        {
            Id = ObjectId.Parse("ab79cde6f69abcd3efab65cd"),
            Name = "Gaming PC",
            Type = WishlistTypes.Product.ToString(),
            CreatedById = ObjectId.Parse("652c3b89ae02a3135d6418fc"),
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
        };
        await wishlistsCollection.InsertOneAsync(gamingPcWishlist);

        var genericWishlist = new Wishlist
        {
            Id = ObjectId.Parse("ab6c2c2d9edf39abcd1ef9ab"),
            Name = "Generic Wishlist Name",
            Type = WishlistTypes.Product.ToString(),
            CreatedById = ObjectId.Parse("652c3b89ae02a3135d6409fc"),
            Messages = new Message[]
            {
                new Message
                {
                    Text = "Prompt",
                    Role = MessageRoles.User.ToString(),
                }
            }
        };
        await wishlistsCollection.InsertOneAsync(genericWishlist);
    }
}
