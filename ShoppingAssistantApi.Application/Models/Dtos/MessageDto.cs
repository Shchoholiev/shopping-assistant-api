namespace ShoppingAssistantApi.Application.Models.Dtos;

public class MessageDto
{
    public string Id { get; set; }

    public string Text { get; set; }

    public string Role { get; set; }

    public string CreatedById { get; set; }
}
