using MongoDB.Bson;
using ShoppingAssistantApi.Domain.Common;

namespace ShoppingAssistantApi.Domain.Entities;

public class Wishlist : EntityBase
{
    public required string Name { get; set; }
    public required string Type { get; set; }
    public ICollection<Message>? Messages { get; set; } = null;

    public required ObjectId UserId { get; set; }
}
