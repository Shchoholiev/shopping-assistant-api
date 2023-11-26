using ShoppingAssistantApi.Application.Models.OpenAi;

namespace ShoppingAssistantApi.Application.IServices;

public interface IOpenAiService
{
    Task<OpenAiMessage> GetChatCompletion(ChatCompletionRequest chat, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a stream of tokens (pieces of words) based on provided chat.
    /// </summary>
    IAsyncEnumerable<string> GetChatCompletionStream(ChatCompletionRequest chat, CancellationToken cancellationToken);
}
