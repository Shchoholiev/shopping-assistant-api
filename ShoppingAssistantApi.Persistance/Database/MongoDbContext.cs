using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace ShoppingAssistantApi.Persistance.Database;

public class MongoDbContext
{
    private readonly MongoClient _client;

    private readonly IMongoDatabase _db;

    public MongoDbContext(IConfiguration configuration)
    {
        this._client = new MongoClient(configuration.GetConnectionString("MongoDb"));
        this._db = this._client.GetDatabase(configuration.GetConnectionString("MongoDatabaseName"));
    }

    public IMongoDatabase Db => this._db;
}