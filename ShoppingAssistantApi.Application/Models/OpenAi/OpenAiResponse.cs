namespace ShoppingAssistantApi.Application.Models.OpenAi;

public class OpenAiResponse
{
    public string Id { get; set; }

    public string Object { get; set; }

    public int Created { get; set; }

    public string Model { get; set; }

    public OpenAiUsage Usage { get; set; }

    public List<OpenAiChoice> Choices { get; set; }
}