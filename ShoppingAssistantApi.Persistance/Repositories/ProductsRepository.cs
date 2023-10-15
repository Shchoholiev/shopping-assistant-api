using ShoppingAssistantApi.Domain.Entities;
using ShoppingAssistantApi.Persistance.Database;

namespace ShoppingAssistantApi.Persistance.Repositories;

public class ProductsRepository : BaseRepository<Product>
{
    public ProductsRepository(MongoDbContext db) : base(db, "Products") { }
}