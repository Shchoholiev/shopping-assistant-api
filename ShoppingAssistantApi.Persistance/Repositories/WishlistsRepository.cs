using System.Linq.Expressions;
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
        return await (await _collection.FindAsync(predicate)).FirstOrDefaultAsync(cancellationToken);
    }
}
