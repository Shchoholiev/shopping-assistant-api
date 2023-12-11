namespace ShoppingAssistantApi.Domain.Enums;

public enum OpenAiRole
{
    System,
    User,
    Assistant
}

public static class OpenAiRoleExtensions
{
    public static string ToRequestString(this OpenAiRole role)
    {
        return role switch
        {
            OpenAiRole.System => "system",
            OpenAiRole.Assistant => "assistant",
            OpenAiRole.User => "user",
            _ => "",
        };
    }
}