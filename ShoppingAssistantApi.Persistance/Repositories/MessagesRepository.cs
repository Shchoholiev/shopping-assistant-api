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

        pageSize = Math.Clamp(pageSize, 1, messageCount);
        var numberOfPages = messageCount / pageSize;

        if (pageNumber > numberOfPages)
        {
            return new List<Message>();
        }

        if (pageNumber < 1)
        {
            throw new ArgumentOutOfRangeException();
        }

        if (pageSize < 1)
        {
            throw new ArgumentOutOfRangeException();
        }

        return await _collection.Find(predicate)
                                .Skip((numberOfPages - pageNumber) * pageSize)
                                .Limit(pageSize)
                                .ToListAsync(cancellationToken);
    }
}
