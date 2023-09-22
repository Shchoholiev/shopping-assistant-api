using Microsoft.Extensions.DependencyInjection;
using ShoppingAssistantApi.Application.IServices;
using ShoppingAssistantApi.Application.IServices.Identity;
using ShoppingAssistantApi.Infrastructure.Services;
using ShoppingAssistantApi.Infrastructure.Services.Identity;

namespace ShoppingAssistantApi.Infrastructure.InfrastructureExtentions;
public static class ServicesExtention
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IRolesService, RolesService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IUserManager, UserManager>();
        services.AddScoped<ITokensService, TokensService>();
        services.AddScoped<IUsersService, UsersService>();

        return services;
    }
}
