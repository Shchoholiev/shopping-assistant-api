using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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

    private readonly ILogger<OpenAiService> _logger;

    public OpenAiService(
        IHttpClientFactory httpClientFactory,
        ILogger<OpenAiService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("OpenAiHttpClient");
        _logger = logger;
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
        _logger.LogInformation($"Sending completion stream request to OpenAI.");

        chat.Stream = true;
        var jsonBody = JsonConvert.SerializeObject(chat, _jsonSettings);
        
        var body = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        using var httpResponse = await _httpClient.PostAsync("", body, cancellationToken);

        using var responseStream = await httpResponse.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(responseStream, Encoding.UTF8);

        while (!cancellationToken.IsCancellationRequested)
        {
            var jsonChunk = await reader.ReadLineAsync();
            
            _logger.LogInformation($"Received chunk from OpenAI.");

            if (jsonChunk.StartsWith("data: "))
            {
                jsonChunk = jsonChunk.Substring("data: ".Length);
                if (jsonChunk == "[DONE]")
                {
                    _logger.LogInformation($"Finished getting response from OpenAI");
                    break;
                }

                var data = JsonConvert.DeserializeObject<OpenAiResponse>(jsonChunk);

                if (data.Choices[0].Delta.Content == "" || data.Choices[0].Delta.Content == null) continue;

                yield return data.Choices[0].Delta.Content;
            }
        }
    }
}