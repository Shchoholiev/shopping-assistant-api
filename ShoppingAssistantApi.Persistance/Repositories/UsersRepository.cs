using MongoDB.Bson;
using MongoDB.Driver;
using ShoppingAssistantApi.Application.GlobalInstances;
using ShoppingAssistantApi.Application.IRepositories;
using ShoppingAssistantApi.Domain.Entities;
using ShoppingAssistantApi.Persistance.Database;
using System.Linq.Expressions;

namespace ShoppingAssistantApi.Persistance.Repositories;

public class UsersRepository : BaseRepository<User>, IUsersRepository
{
    public UsersRepository(MongoDbContext db) : base(db, "Users") { }

    public async Task<User> GetUserAsync(ObjectId id, CancellationToken cancellationToken)
    {
        return await (await this._collection.FindAsync(x => x.Id == id && x.IsDeleted == false)).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<User> GetUserAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken)
    {
        return await (await this._collection.FindAsync(Builders<User>.Filter.Where(predicate) & Builders<User>.Filter.Where(x => !x.IsDeleted))).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<User> UpdateUserAsync(User user, CancellationToken cancellationToken)
    {
        var updateDefinition = Builders<User>.Update
            .Set(u => u.Email, user.Email)
            .Set(u => u.Phone, user.Phone)
            .Set(u => u.GuestId, user.GuestId)
            .Set(u => u.Roles, user.Roles)
            .Set(u => u.PasswordHash, user.PasswordHash)
            .Set(u => u.LastModifiedDateUtc, DateTime.UtcNow)
            .Set(u => u.LastModifiedById, GlobalUser.Id);

        var options = new FindOneAndUpdateOptions<User>
        {
            ReturnDocument = ReturnDocument.After
        };

        return await this._collection.FindOneAndUpdateAsync(
            Builders<User>.Filter.Eq(u => u.Id, user.Id) & Builders<User>.Filter.Where(x => !x.IsDeleted), updateDefinition, options, cancellationToken);
    }
}
