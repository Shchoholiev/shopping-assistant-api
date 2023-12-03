namespace ShoppingAssistantApi.Api.ApiExtentions;

public static class CorsExtension
{
    public static IServiceCollection AddCorsAllowAny(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("allowAnyOrigin",
            builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        return services;
    }
}
