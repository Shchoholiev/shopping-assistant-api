using ShoppingAssistantApi.Api.CustomMiddlewares;

namespace ShoppingAssistantApi.Api.ApiExtentions;

public static class GlobalUserExtention
{
    public static IApplicationBuilder AddGlobalUserMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<GlobalUserCustomMiddleware>();
        return app;
    }
}
