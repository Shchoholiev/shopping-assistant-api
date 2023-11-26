using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Driver;
using ShoppingAssistantApi.Application.IRepositories;
using ShoppingAssistantApi.Domain.Entities;
using ShoppingAssistantApi.Persistance.Database;

namespace ShoppingAssistantApi.Persistance.Repositories;

public class WishlistsRepository : BaseRepository<Wishlist>, IWishlistsRepository
{
    public WishlistsRepository(MongoDbContext db) : base(db, "Wishlists") { }

    public async Task<Wishlist> GetWishlistAsync(Expression<Func<Wishlist, bool>> predicate, CancellationToken cancellationToken)
    {
        return await (await _collection.FindAsync(Builders<Wishlist>.Filter.Where(predicate) & Builders<Wishlist>.Filter.Where(x => !x.IsDeleted))).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Wishlist> UpdateWishlistNameAsync(ObjectId wishlistId, string newName,
            ObjectId updatedById, CancellationToken cancellationToken)
    {
        var filterDefinition = Builders<Wishlist>.Filter.Eq(w => w.Id, wishlistId) & Builders<Wishlist>.Filter.Where(x => !x.IsDeleted);

        var updateDefinition = Builders<Wishlist>.Update
            .Set(w => w.Name, newName)
            .Set(w => w.LastModifiedDateUtc, DateTime.UtcNow)
            .Set(w => w.LastModifiedById, updatedById);

        var options = new FindOneAndUpdateOptions<Wishlist>
        {
            ReturnDocument = ReturnDocument.After
        };

        return await _collection.FindOneAndUpdateAsync(filterDefinition, updateDefinition, options, cancellationToken);
    }
}
