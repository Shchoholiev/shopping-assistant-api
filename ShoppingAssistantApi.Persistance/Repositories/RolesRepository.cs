using MongoDB.Bson;
using MongoDB.Driver;
using ShoppingAssistantApi.Application.IRepositories;
using ShoppingAssistantApi.Domain.Entities;
using ShoppingAssistantApi.Persistance.Database;
using System.Linq.Expressions;

namespace ShoppingAssistantApi.Persistance.Repositories;

public class RolesRepository : BaseRepository<Role>, IRolesRepository
{
    public RolesRepository(MongoDbContext db) : base(db, "Roles") { }

    public async Task<Role> GetRoleAsync(ObjectId id, CancellationToken cancellationToken)
    {
        return await (await this._collection.FindAsync(x => x.Id == id && x.IsDeleted == false)).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Role> GetRoleAsync(Expression<Func<Role, bool>> predicate, CancellationToken cancellationToken)
    {
        return await (await this._collection.FindAsync(predicate)).FirstOrDefaultAsync(cancellationToken);
    }
}