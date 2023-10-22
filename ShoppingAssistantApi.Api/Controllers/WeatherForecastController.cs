using Microsoft.AspNetCore.Mvc;
using ShoppingAssistantApi.Application.IServices;
using ShoppingAssistantApi.Application.Models.OpenAi;
using ShoppingAssistantApi.Domain.Enums;

namespace ShoppingAssistantApi.Api.Controllers;
[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    private readonly IOpenAiService _openAiService;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IOpenAiService openAiService)
    {
        _openAiService = openAiService;
        _logger = logger;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

    [HttpPost("open-ai-test-simple")]
    public async Task<OpenAiMessage> OpenAiTest(string text)
    {
        return await _openAiService.GetChatCompletion(new ChatCompletionRequest 
        {
            Messages = new List<OpenAiMessage>
            {
                new OpenAiMessage
                {
                    Role = OpenAiRole.System.RequestConvert(),
                    Content = "You are a Shopping Assistant that helps people find product recommendations. Ask user additional questions if more context needed.\nYou must return data with one of the prefixes:\n[Question] - return question. Each question must have suggestions.\n[Options] - return semicolon separated suggestion how to answer to a question\n[Message] - return text\n[Products] - return semicolon separated product names"
                },

                new OpenAiMessage
                {
                    Role = OpenAiRole.Assistant.RequestConvert(),
                    Content = "[Question] What are you looking for?\n[Options] Bicycle, Laptop"
                },

                new OpenAiMessage
                {
                    Role = OpenAiRole.User.RequestConvert(),
                    Content = text
                }
            }
        }, CancellationToken.None);
    }

    [HttpPost("open-ai-test-streamed")]
    public IAsyncEnumerable<string> OpenAiTestStrean(string text)
    {
        return _openAiService.GetChatCompletionStream(new ChatCompletionRequest
        {
            Messages = new List<OpenAiMessage>
            {
                new OpenAiMessage
                {
                    Role = OpenAiRole.System.RequestConvert(),
                    Content = "You are a Shopping Assistant that helps people find product recommendations. Ask user additional questions if more context needed.\nYou must return data with one of the prefixes:\n[Question] - return question. Each question must have suggestions.\n[Options] - return semicolon separated suggestion how to answer to a question\n[Message] - return text\n[Products] - return semicolon separated product names"
                },

                new OpenAiMessage
                {
                    Role = OpenAiRole.Assistant.RequestConvert(),
                    Content = "[Question] What are you looking for?\n[Options] Bicycle, Laptop"
                },

                new OpenAiMessage
                {
                    Role = OpenAiRole.User.RequestConvert(),
                    Content = text
                }
            }
        }, CancellationToken.None);
    }
}