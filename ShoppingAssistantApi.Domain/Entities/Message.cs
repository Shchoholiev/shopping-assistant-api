using MongoDB.Bson;
using ShoppingAssistantApi.Domain.Common;

namespace ShoppingAssistantApi.Domain.Entities;

public class Message : EntityBase
{
    public string Text { get; set; }

    public string Role { get; set; }

    public ObjectId WishlistId { get; set; }
}
