using ShoppingAssistantApi.Domain.Common;

namespace ShoppingAssistantApi.Domain.Entities;

public class Wishlist : EntityBase
{
    public string Name { get; set; }

    public string Type { get; set; }

    public ICollection<Message>? Messages { get; set; }
}
