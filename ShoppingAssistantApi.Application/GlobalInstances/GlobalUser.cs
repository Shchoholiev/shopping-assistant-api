using MongoDB.Bson;

namespace ShoppingAssistantApi.Application.GlobalInstances;
public static class GlobalUser
{
    public static ObjectId? Id { get; set; }

    public static string? Email { get; set; }

    public static string? Phone { get; set; }

    public static List<string>? Roles { get; set; } = new List<string>();
}