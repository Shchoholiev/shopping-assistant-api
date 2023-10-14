using ShoppingAssistantApi.Application.Models.OpenAi;

namespace ShoppingAssistantApi.Application.IServices;

public interface IOpenAiService
{
    Task<OpenAiResponse?> GetChatCompletion(ChatCompletionRequest chat, CancellationToken cancellationToken);

    IAsyncEnumerable<OpenAiResponse> GetChatCompletionStream(ChatCompletionRequest chat, CancellationToken cancellationToken);
}