using MongoDB.Bson;
using ShoppingAssistantApi.Domain.Common;

namespace ShoppingAssistantApi.Domain.Entities;

public class Product : EntityBase
{
    public ObjectId WishlistId { get; set; }
    
    public string? Url { get; set; }
    
    public string? Name { get; set; }
    
    public string? Description { get; set; }
    
    public double Rating { get; set; }
    
    public string[]? ImagesUrls { get; set; }
    
    public bool WasOpened { get; set; }
}