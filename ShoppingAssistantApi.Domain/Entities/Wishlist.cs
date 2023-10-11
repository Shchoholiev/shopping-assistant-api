using ShoppingAssistantApi.Domain.Common;

namespace ShoppingAssistantApi.Domain.Entities;

public class Wishlist : EntityBase
{
    public required string Name { get; set; }

    public required string Type { get; set; }

    public ICollection<Message>? Messages { get; set; }
}
