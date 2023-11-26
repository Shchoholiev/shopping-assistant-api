using ShoppingAssistantApi.Domain.Enums;

namespace ShoppingAssistantApi.Application.Models.OpenAi;

public class OpenAiMessage
{
    public string Role { get; set; }

    public string Content { get; set; }
}
