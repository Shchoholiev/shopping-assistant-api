namespace ShoppingAssistantApi.Domain.Enums;

public enum OpenAiRole
{
    System,
    User,
    Assistant
}

public static class OpenAiRoleExtensions
{
    public static string RequestConvert(this OpenAiRole role)
    {
        switch (role)
        {
            case OpenAiRole.System:
                return "system";
            case OpenAiRole.Assistant:
                return "assistant";
            case OpenAiRole.User:
                return "user";
            default:
                return "";
        }
    }
}