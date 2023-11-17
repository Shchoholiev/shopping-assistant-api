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
        return await _collection.Find(Builders<Message>.Filter.Where(predicate) & Builders<Message>.Filter.Where(x => !x.IsDeleted))
                                .SortByDescending(x => x.CreatedDateUtc)
                                .Skip((pageNumber - 1) * pageSize)
                                .Limit(pageSize)
                                .ToListAsync(cancellationToken);
    }
}
