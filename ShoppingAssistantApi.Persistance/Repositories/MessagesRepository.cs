using ShoppingAssistantApi.Application.IRepositories;
using ShoppingAssistantApi.Domain.Entities;
using ShoppingAssistantApi.Persistance.Database;

namespace ShoppingAssistantApi.Persistance.Repositories;

public class MessagesRepository : BaseRepository<Message>, IMessagesRepository
{
    public MessagesRepository(MongoDbContext db) : base(db, "Messages") { }
}
