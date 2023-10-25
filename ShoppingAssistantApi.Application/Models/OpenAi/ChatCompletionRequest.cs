namespace ShoppingAssistantApi.Application.Models.OpenAi;

public class ChatCompletionRequest
{
    public string Model { get; set; } = "gpt-4";

    public List<OpenAiMessage> Messages { get; set; }

    public double Temperature { get; set; } = 0.7;

    public int MaxTokens { get; set; } = 256;

    public bool Stream { get; set; } = false;
}
