using System.Net.Http.Headers;
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
        var request = new HttpRequestMessage(HttpMethod.Post, "")
        {
            Content = body
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

        using var httpResponse = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        var allData = string.Empty;

        using var streamReader = new StreamReader(await httpResponse.Content.ReadAsStreamAsync(cancellationToken));
        while (!streamReader.EndOfStream)
        {
            var line = await streamReader.ReadLineAsync(cancellationToken);
            allData += line + "\n\n";
            if (string.IsNullOrEmpty(line)) continue;

            var json = line?.Substring(6, line.Length - 6);
            if (json == "[DONE]")
            {
                yield return json;
                yield break;
            }

            var data = JsonConvert.DeserializeObject<OpenAiResponse>(json);

            if (data.Choices[0].Delta.Content == "" || data.Choices[0].Delta.Content == null) continue;

            yield return data.Choices[0].Delta.Content;
        }
    }
}