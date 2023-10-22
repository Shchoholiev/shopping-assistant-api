using System.IO;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using ShoppingAssistantApi.Application.IServices;
using ShoppingAssistantApi.Application.Models.OpenAi;

namespace ShoppingAssistantApi.Infrastructure.Services;

public class OpenAiService : IOpenAiService
{

    private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
    {
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
        },
        NullValueHandling = NullValueHandling.Ignore,
    };

    private readonly HttpClient _httpClient;

    public OpenAiService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("OpenAiHttpClient");
    }

    public async Task<OpenAiMessage> GetChatCompletion(ChatCompletionRequest chat, CancellationToken cancellationToken)
    {
        chat.Stream = false; 
        var jsonBody = JsonConvert.SerializeObject(chat, _jsonSettings);
        var body = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        using var httpResponse = await _httpClient.PostAsync("", body, cancellationToken);

        var responseBody = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

        var data = JsonConvert.DeserializeObject<OpenAiResponse>(responseBody);

        return data.Choices[0].Message;
    }

    public async IAsyncEnumerable<string> GetChatCompletionStream(ChatCompletionRequest chat, CancellationToken cancellationToken)
    {
        chat.Stream = true;
        var jsonBody = JsonConvert.SerializeObject(chat, _jsonSettings);
        
        var body = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        using var httpResponse = await _httpClient.PostAsync("", body, cancellationToken);

        using var responseStream = await httpResponse.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(responseStream, Encoding.UTF8);

        while (!cancellationToken.IsCancellationRequested)
        {
            var jsonChunk = await reader.ReadLineAsync();
            if (jsonChunk.StartsWith("data: "))
            {
                jsonChunk = jsonChunk.Substring("data: ".Length);
                if (jsonChunk == "[DONE]") break;
                var data = JsonConvert.DeserializeObject<OpenAiResponse>(jsonChunk);
                if (data.Choices[0].Delta.Content == "" || data.Choices[0].Delta.Content == null) continue;
                yield return data.Choices[0].Delta.Content;
            }
        }
    }
}