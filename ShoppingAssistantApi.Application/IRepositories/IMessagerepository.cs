using System.Linq.Expressions;
using MongoDB.Bson;
using ShoppingAssistantApi.Domain.Entities;

namespace ShoppingAssistantApi.Application.IRepositories;

public interface IMessagesRepository : IBaseRepository<Message>
{
    Task<List<Message>> GetPageStartingFromEndAsync(int pageNumber, int pageSize, Expression<Func<Message, bool>> predicate, CancellationToken cancellationToken);

    Task<List<Message>> GetWishlistMessagesAsync(ObjectId wishlistId, CancellationToken cancellationToken);
}
