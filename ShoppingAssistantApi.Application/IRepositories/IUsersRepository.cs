using MongoDB.Bson;
using ShoppingAssistantApi.Domain.Entities;
using System.Linq.Expressions;

namespace ShoppingAssistantApi.Application.IRepositories;

public interface IUsersRepository : IBaseRepository<User>
{
    Task<User> GetUserAsync(ObjectId id, CancellationToken cancellationToken);

    Task<User> GetUserAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken);

    Task<User> UpdateUserAsync(User user, CancellationToken cancellationToken);
}

