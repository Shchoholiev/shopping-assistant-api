namespace ShoppingAssistantApi.Domain.Enums;

public enum SearchEventType
{
    Wishlist = 0,
    Message = 1,
    Suggestion = 2,
    Product = 3
}

public static class SearchEventTypeExtensions
{
    public static string ToSseEventString(this SearchEventType eventType)
    {
        return eventType switch
        {
            SearchEventType.Wishlist => "wishlist",
            SearchEventType.Message => "message",
            SearchEventType.Suggestion => "suggestion",
            SearchEventType.Product => "product",
            _ => throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null),
        };
    }
}
