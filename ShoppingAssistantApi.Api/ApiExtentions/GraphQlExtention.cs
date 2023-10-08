using ShoppingAssistantApi.Api.Queries;
using ShoppingAssistantApi.Api.Mutations;

namespace ShoppingAssistantApi.Api.ApiExtentions;

public static class GraphQlExtention
{
    public static IServiceCollection AddGraphQl(this IServiceCollection services)
    {
        services
            .AddGraphQLServer()
            .AddQueryType()
                .AddTypeExtension<UsersQuery>()
                .AddTypeExtension<RolesQuery>()
            .AddMutationType()
                .AddTypeExtension<AccessMutation>()
                .AddTypeExtension<UsersMutation>()
                .AddTypeExtension<RolesMutation>()
                .AddTypeExtension<WishlistsMutation>()
            .AddAuthorization()
            .InitializeOnStartup(keepWarm: true);

        return services;
    }
}
