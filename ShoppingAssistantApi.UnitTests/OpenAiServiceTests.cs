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

    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;

    public OpenAiServiceTests()
    {
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        var client = new HttpClient(_mockHttpMessageHandler.Object);
        client.BaseAddress = new Uri("https://www.google.com.ua/");

        _mockHttpClientFactory
            .Setup(factory => factory.CreateClient(It.IsAny<string>()))
            .Returns(() =>
            {
                return client;
            });
        
        _openAiService = new OpenAiService(_mockHttpClientFactory.Object);
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
                                ""content"": ""Hello, World!""
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
                    Role = OpenAiRole.User.RequestConvert(),
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

    [Fact]
    public async Task GetChatCompletionStream_ValidChat_ReturnsNewMessage()
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
                    Role = OpenAiRole.User.RequestConvert(),
                    Content = "Return Hello World!"
                }
            }
        };

        // Act
        var newMessage = _openAiService.GetChatCompletionStream(chat, CancellationToken.None);

        // Assert
        Assert.NotNull(newMessage);
        Assert.Equal("Hello World!", newMessage.ToString());
    }
}