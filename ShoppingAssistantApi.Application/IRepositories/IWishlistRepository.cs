using System.Linq.Expressions;
using ShoppingAssistantApi.Domain.Entities;

namespace ShoppingAssistantApi.Application.IRepositories;

public interface IWishlistsRepository : IBaseRepository<Wishlist>
{
    public Task<Wishlist> GetWishlistAsync(Expression<Func<Wishlist, bool>> predicate, CancellationToken cancellationToken);
}
