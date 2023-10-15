using MongoDB.Bson;
using ShoppingAssistantApi.Domain.Common;

namespace ShoppingAssistantApi.Domain.Entities;

public class Product : EntityBase
{
    public required string Url { get; set; }

    public required string Name { get; set; }

    public required string Description { get; set; }

    public required double Rating { get; set; }

    public required string[] ImagesUrls { get; set; }

    public required bool WasOpened { get; set; }

    public required ObjectId WishlistId { get; set; }
}
