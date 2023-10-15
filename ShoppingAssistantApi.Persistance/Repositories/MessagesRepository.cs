using System.Linq.Expressions;
using MongoDB.Driver;
using ShoppingAssistantApi.Application.IRepositories;
using ShoppingAssistantApi.Domain.Entities;
using ShoppingAssistantApi.Persistance.Database;

namespace ShoppingAssistantApi.Persistance.Repositories;

public class MessagesRepository : BaseRepository<Message>, IMessagesRepository
{
    public MessagesRepository(MongoDbContext db) : base(db, "Messages") { }

    public async Task<List<Message>> GetPageStartingFromEndAsync(int pageNumber, int pageSize, Expression<Func<Message, bool>> predicate, CancellationToken cancellationToken)
    {
        var messageCount = await GetCountAsync(predicate, cancellationToken);

        return await _collection.Find(predicate)
                                .Skip((messageCount / pageSize - pageNumber) * pageSize)
                                .Limit(pageSize)
                                .ToListAsync(cancellationToken);
    }
}
