using System.Linq.Expressions;
using MongoDB.Bson;
using ShoppingAssistantApi.Domain.Entities;

namespace ShoppingAssistantApi.Application.IRepositories;

public interface IWishlistsRepository : IBaseRepository<Wishlist>
{
    Task<Wishlist> GetWishlistAsync(Expression<Func<Wishlist, bool>> predicate, CancellationToken cancellationToken);

    Task<Wishlist> UpdateWishlistNameAsync(ObjectId id, string name, ObjectId updatedById, CancellationToken cancellationToken);
}
