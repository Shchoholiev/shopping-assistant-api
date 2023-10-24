using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShoppingAssistantApi.Application.IServices;
using ShoppingAssistantApi.Application.IServices.Identity;
using ShoppingAssistantApi.Infrastructure.Services;
using ShoppingAssistantApi.Infrastructure.Services.Identity;
using System.Net.Http.Headers;

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
        services.AddScoped<IWishlistsService, WishlistsService>();
        services.AddScoped<IOpenAiService, OpenAiService>();
        services.AddScoped<IProductService, ProductService>();

        return services;
    }

    public static IServiceCollection AddHttpClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient(
            "OpenAiHttpClient",
            client =>
            {
                client.BaseAddress = new Uri(configuration.GetValue<string>("ApiUri"));
                client.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", configuration.GetValue<string>("ApiKey"));
            });

        return services;
    }
}
