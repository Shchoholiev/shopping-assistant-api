using ShoppingAssistantApi.Domain.Enums;

namespace ShoppingAssistantApi.Application.Models.ProductSearch;

public class ServerSentEvent
{
    public SearchEventType Event { get; set; }

    public string Data { get; set; }
}
