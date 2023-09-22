using ShoppingAssistantApi.Domain.Common;
using System.Linq.Expressions;

namespace ShoppingAssistantApi.Application.IRepositories;

public interface IBaseRepository<TEntity> where TEntity : EntityBase
{
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken);

    Task<List<TEntity>> GetPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);

    Task<List<TEntity>> GetPageAsync(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);

    Task<int> GetTotalCountAsync();

    Task<int> GetCountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);

    Task<TEntity> DeleteAsync(TEntity entity, CancellationToken cancellationToken);
}