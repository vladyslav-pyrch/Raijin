using Raijin.CombinatoricsService.Api.Endpoints;

namespace Raijin.CombinatoricsService.Api;

public static class AddEndpointsExtension
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services)
    {
        // Register all IEndpoint implementations in the assembly
        IEnumerable<Type> endpointTypes = typeof(Program).Assembly.GetTypes()
            .Where(type =>
                typeof(IEndpoint).IsAssignableFrom(type) && type is { IsInterface: false, IsAbstract: false });

        foreach (Type endpointType in endpointTypes)
            services.AddSingleton(typeof(IEndpoint), endpointType);

        return services;
    }

    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        // Resolve all IEndpoint implementations and call their Map method
        using IServiceScope scope = app.ServiceProvider.CreateScope();

        IEnumerable<IEndpoint> endpoints = scope.ServiceProvider.GetServices<IEndpoint>();

        foreach (IEndpoint endpoint in endpoints)
            endpoint.Map((WebApplication)app);

        return app;
    }
}