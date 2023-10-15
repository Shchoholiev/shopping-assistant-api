namespace ShoppingAssistantApi.Application.Models.Dtos;

public class MessageDto
{
    public required string Id { get; set; }

    public required string Text { get; set; }

    public required string Role { get; set; }

    public required string CreatedById { get; set; }
}
