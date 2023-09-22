using Microsoft.Extensions.DependencyInjection;
using ShoppingAssistantApi.Application.MappingProfiles;
using System.Reflection;

namespace ShoppingAssistantApi.Application.ApplicationExtentions;

public static class MapperExtension
{
    public static IServiceCollection AddMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetAssembly(typeof(UserProfile)));

        return services;
    }
}
