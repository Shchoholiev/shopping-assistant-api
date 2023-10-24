using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Driver;
using ShoppingAssistantApi.Application.GlobalInstances;
using ShoppingAssistantApi.Application.IRepositories;
using ShoppingAssistantApi.Domain.Entities;
using ShoppingAssistantApi.Persistance.Database;

namespace ShoppingAssistantApi.Persistance.Repositories;

public class WishlistsRepository : BaseRepository<Wishlist>, IWishlistsRepository
{
    public WishlistsRepository(MongoDbContext db) : base(db, "Wishlists") { }

    public async Task<Wishlist> GetWishlistAsync(Expression<Func<Wishlist, bool>> predicate, CancellationToken cancellationToken)
    {
        return await (await _collection.FindAsync(predicate)).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Wishlist> UpdateWishlistNameAsync(ObjectId wishlistId, string newName, CancellationToken cancellationToken)
    {
        var filterDefinition = Builders<Wishlist>.Filter.Eq(w => w.Id, wishlistId);

        var updateDefinition = Builders<Wishlist>.Update
            .Set(w => w.Name, newName)
            .Set(w => w.LastModifiedDateUtc, DateTime.UtcNow)
            .Set(w => w.LastModifiedById, GlobalUser.Id);

        var options = new FindOneAndUpdateOptions<Wishlist>
        {
            ReturnDocument = ReturnDocument.After
        };

        return await _collection.FindOneAndUpdateAsync(filterDefinition, updateDefinition, options, cancellationToken);
    }
}
