using System.Net;
using Moq;
using Moq.Protected;
using ShoppingAssistantApi.Application.IServices;
using ShoppingAssistantApi.Application.Models.OpenAi;
using ShoppingAssistantApi.Domain.Enums;
using ShoppingAssistantApi.Infrastructure.Services;

namespace ShoppingAssistantApi.UnitTests;

public class OpenAiServiceTests
{
    private readonly IOpenAiService _openAiService;

    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;

    private readonly HttpClient _httpClient;

    public OpenAiServiceTests()
    {
        // Mock any dependencies
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _openAiService = new OpenAiService(_httpClient);
    }

    [Fact]
    public async Task GetChatCompletion_ValidChat_ReturnsNewMessage()
    {
        // Arrange
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"
                {
                    ""id"": ""chatcmpl-89OMdgTZXOLAXv7bPUJ4SwrPpS5Md"",
                    ""object"": ""chat.completion"",
                    ""created"": 1697249299,
                    ""model"": ""gpt-3.5-turbo-0613"",
                    ""choices"": [
                        {
                            ""index"": 0,
                            ""message"": {
                                ""role"": ""assistant"",
                                ""content"": ""Hello World!""
                            },
                            ""finish_reason"": ""stop""
                        }
                    ],
                    ""usage"": {
                        ""prompt_tokens"": 10,
                        ""completion_tokens"": 3,
                        ""total_tokens"": 13
                    }
                }"),
            });
            
        var chat = new ChatCompletionRequest
        {
            Messages = new List<OpenAiMessage>
            {
                new OpenAiMessage
                {
                    Role = OpenAiRole.User,
                    Content = "Return Hello World!"
                }
            }
        };

        // Act
        var newMessage = await _openAiService.GetChatCompletion(chat, CancellationToken.None);

        // Assert
        Assert.NotNull(newMessage);
        Assert.Equal("Hello, World!", newMessage.Content);
    }

    // TODO: Add more tests
}