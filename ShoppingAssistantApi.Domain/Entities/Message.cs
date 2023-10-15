using MongoDB.Bson;
using ShoppingAssistantApi.Domain.Common;

namespace ShoppingAssistantApi.Domain.Entities;

public class Message : EntityBase
{
    public required string Text { get; set; }

    public required string Role { get; set; }

    public required ObjectId WishlistId { get; set; }
}
