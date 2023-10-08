using ShoppingAssistantApi.Application.IRepositories;
using ShoppingAssistantApi.Domain.Entities;
using ShoppingAssistantApi.Persistance.Database;

namespace ShoppingAssistantApi.Persistance.Repositories;

public class WishlistsRepository : BaseRepository<Wishlist>, IWishlistsRepository
{
    public WishlistsRepository(MongoDbContext db) : base(db, "Wishlists") { }
}
