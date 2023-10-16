using ShoppingAssistantApi.Application.IRepositories;
using ShoppingAssistantApi.Domain.Entities;
using ShoppingAssistantApi.Persistance.Database;

namespace ShoppingAssistantApi.Persistance.Repositories;

public class RefreshTokensRepository : BaseRepository<RefreshToken>, IRefreshTokensRepository
{
    public RefreshTokensRepository(MongoDbContext db) : base(db, "RefreshTokens") { }
}
