using MongoDB.Bson;
using MongoDB.Driver;
using ShoppingAssistantApi.Application.IRepositories;
using ShoppingAssistantApi.Domain.Common;
using ShoppingAssistantApi.Persistance.Database;
using System.Linq.Expressions;

namespace ShoppingAssistantApi.Persistance.Repositories;

public abstract class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : EntityBase
{
    protected MongoDbContext _db;

    protected IMongoCollection<TEntity> _collection;

    public BaseRepository(MongoDbContext db, string collectionName)
    {
        this._db = db;
        this._collection = _db.Db.GetCollection<TEntity>(collectionName);
    }

    public async Task<TEntity> GetOneAsync(ObjectId id, CancellationToken cancellationToken)
    {
        return await this._collection.Find(x => x.Id == id && x.IsDeleted == false).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TEntity> GetOneAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        return await this._collection.Find(predicate).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken)
    {
        await this._collection.InsertOneAsync(entity, new InsertOneOptions(), cancellationToken);
        return entity;
    }

    public async Task<List<TEntity>> GetPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        return await this._collection.Find(Builders<TEntity>.Filter.Empty)
                                     .Skip((pageNumber - 1) * pageSize)
                                     .Limit(pageSize)
                                     .ToListAsync(cancellationToken);
    }

    public async Task<List<TEntity>> GetPageAsync(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        return await this._collection.Find(predicate)
                                     .Skip((pageNumber - 1) * pageSize)
                                     .Limit(pageSize)
                                     .ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalCountAsync()
    {
        return (int)(await this._collection.EstimatedDocumentCountAsync());
    }

    public async Task<int> GetCountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        return (int)(await this._collection.CountDocumentsAsync(predicate, cancellationToken: cancellationToken));
    }

    public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        return await this._collection.Find(predicate).AnyAsync(cancellationToken);
    }

    public async Task<TEntity> DeleteAsync(TEntity entity, CancellationToken cancellationToken)
    {
        var updateDefinition = Builders<TEntity>.Update
        .Set(e => e.IsDeleted, true)
        .Set(e => e.LastModifiedById, entity.LastModifiedById)
        .Set(e => e.LastModifiedDateUtc, entity.LastModifiedDateUtc);

        var options = new FindOneAndUpdateOptions<TEntity>
        {
            ReturnDocument = ReturnDocument.After
        };

        return await this._collection.FindOneAndUpdateAsync(
            Builders<TEntity>.Filter.Eq(e => e.Id, entity.Id), updateDefinition, options, cancellationToken);
    }
}
