namespace ShoppingAssistantApi.Application.Models.OpenAi;

public class OpenAiChoice
{
    public OpenAiMessage Message { get; set; }
    
    public OpenAiDelta Delta { get; set; }

    public string FinishReason { get; set; }
    
    public int Index { get; set; }
}