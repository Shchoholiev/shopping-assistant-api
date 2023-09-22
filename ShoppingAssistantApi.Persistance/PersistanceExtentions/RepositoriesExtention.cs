using Microsoft.Extensions.DependencyInjection;
using ShoppingAssistantApi.Application.IRepositories;
using ShoppingAssistantApi.Persistance.Database;
using ShoppingAssistantApi.Persistance.Repositories;

namespace ShoppingAssistantApi.Persistance.PersistanceExtentions;

public static class RepositoriesExtention
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<MongoDbContext>();

        services.AddScoped<IRolesRepository, RolesRepository>();
        services.AddScoped<IUsersRepository, UsersRepository>();
        
        return services;
    }
}
