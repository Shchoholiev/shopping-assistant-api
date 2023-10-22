using ShoppingAssistantApi.Domain.Common;

namespace ShoppingAssistantApi.Domain.Entities;

public class User : EntityBase
{
    public Guid GuestId { get; set; }

    public List<Role> Roles { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? PasswordHash { get; set; }
}
