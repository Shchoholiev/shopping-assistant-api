namespace ShoppingAssistantApi.Application.Exceptions;

public class InvalidEmailException : Exception
{
    public InvalidEmailException() { }

    public InvalidEmailException(string email) : base(String.Format($"String {email} can not be an email.")) { }
}
