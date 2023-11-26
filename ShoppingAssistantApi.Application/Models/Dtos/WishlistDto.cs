namespace ShoppingAssistantApi.Application.Models.Dtos;

public class WishlistDto
{
    public required string Id { get; set; }

    public required string Name { get; set; }

    public required string Type { get; set; }

    public required string CreatedById { get; set; }
}
