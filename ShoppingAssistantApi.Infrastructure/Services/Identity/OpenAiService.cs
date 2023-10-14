using System;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ShoppingAssistantApi.Application.IServices;
using ShoppingAssistantApi.Application.Models.OpenAi;

namespace ShoppingAssistantApi.Infrastructure.Services;

public class OpenaiService : IOpenAiService
{
    private readonly HttpClient _httpClient;

    private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
    {
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
        },
        NullValueHandling = NullValueHandling.Ignore,
    };

    private readonly IConfiguration _configuration;

    //private readonly OpenAIClient _openAiClient;

    private readonly ILogger _logger;

    public OpenaiService(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<OpenaiService> logger
    )
    {
        _httpClient = httpClientFactory.CreateClient("OpenAiHttpClient");
        _configuration = configuration;

        //var openAIApiKey = _configuration.GetSection("OpenAi")?.GetValue<string>("ApiKey");
        //_openAiClient = new OpenAIClient(openAIApiKey, new OpenAIClientOptions());

        _logger = logger;
    }

    public async IAsyncEnumerable<OpenAiResponse> GetChatCompletionStream(ChatCompletionRequest chat, CancellationToken cancellationToken)
    {
        chat.Stream = true;
        var jsonBody = JsonConvert.SerializeObject(chat, _jsonSettings);
        _logger.LogInformation(jsonBody);
        var body = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = body
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

        using var httpResponse = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        var allData = string.Empty;

        using var streamReader = new StreamReader(await httpResponse.Content.ReadAsStringAsync(cancellationToken));
        while(!streamReader.EndOfStream)
        {
            var line = await streamReader.ReadLineAsync();
            allData += line + "\n\n";
            if (string.IsNullOrEmpty(line)) continue;

            var json = line?.Substring(6, line.Length - 6);
            if (json == "[DONE]") yield break;

            var OpenAiResponse = JsonConvert.DeserializeObject<OpenAiResponse>(json, _jsonSettings);
            yield return OpenAiResponse;
        }
    }

    public async Task<OpenAiResponse?> GetChatCompletion(ChatCompletionRequest chat, CancellationToken cancellationToken)
    {
        chat.Stream = false; 
        var jsonBody = JsonConvert.SerializeObject(chat, _jsonSettings);
        var body = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        _logger.LogInformation(jsonBody);
        using var httpResponse = await _httpClient.PostAsync("chat/completions", body, cancellationToken);

        var responseBody = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

        var responses = new List<OpenAiResponse>();
        foreach (var line in responseBody.Split(new[] {"\n\n"}, StringSplitOptions.RemoveEmptyEntries))
        {
            if (line.Trim() == "[DONE]") break;

            var json = line.Substring(6);
            var OpenAiResponse = JsonConvert.DeserializeObject<OpenAiResponse>(json, _jsonSettings);
            responses.Add(OpenAiResponse);
        }

        return responses.Count > 0 ? responses.Last() : null;
    }
}