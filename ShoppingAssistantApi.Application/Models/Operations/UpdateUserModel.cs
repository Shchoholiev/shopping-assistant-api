using ShoppingAssistantApi.Application.Models.Dtos;
using ShoppingAssistantApi.Application.Models.Identity;

namespace ShoppingAssistantApi.Application.Models.Operations;

public class UpdateUserModel
{
    public TokensModel Tokens { get; set; }

    public UserDto User { get; set; }
}
