using System;
using System.Net.Http.Headers;
using System.Text;
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

    public OpenAiService(HttpClient client)
    {
        _httpClient = client;
    }

    

    public async Task<OpenAiMessage> GetChatCompletion(ChatCompletionRequest chat, CancellationToken cancellationToken)
    {
        chat.Stream = false; 
        var jsonBody = JsonConvert.SerializeObject(chat, _jsonSettings);
        var body = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        
        using var httpResponse = await _httpClient.PostAsync("chat/completions", body, cancellationToken);

        var responseBody = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

        var responses = new List<OpenAiMessage>();
        foreach (var line in responseBody.Split(new[] {"\n\n"}, StringSplitOptions.RemoveEmptyEntries))
        {
            if (line.Trim() == "[DONE]") break;

            var json = line.Substring(6);
            var OpenAiMessage = JsonConvert.DeserializeObject<OpenAiMessage>(json, _jsonSettings);
            responses.Add(OpenAiMessage);
        }

        return responses.Count > 0 ? responses.Last() : null;
    }

    public async IAsyncEnumerable<string> GetChatCompletionStream(ChatCompletionRequest chat, CancellationToken cancellationToken)
    {
        chat.Stream = true;
        var jsonBody = JsonConvert.SerializeObject(chat, _jsonSettings);
        
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

            var OpenAiResponse = JsonConvert.DeserializeObject<string>(json, _jsonSettings);
            yield return OpenAiResponse;
        }
    }
}