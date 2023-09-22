using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ShoppingAssistantApi.Domain.Common;

public abstract class EntityBase
{
    [BsonId]
    public ObjectId Id { get; set; }

    public ObjectId CreatedById { get; set; }

    public DateTime CreatedDateUtc { get; set; }

    public bool IsDeleted { get; set; }

    public ObjectId? LastModifiedById { get; set; }

    public DateTime? LastModifiedDateUtc { get; set; }
}