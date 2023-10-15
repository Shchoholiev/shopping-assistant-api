using ShoppingAssistantApi.Domain.Enums;

namespace ShoppingAssistantApi.Application.Models.OpenAi;

public class OpenAiMessage
{
    public OpenAiRole Role { get; set; }

    public string Content { get; set; }
}
