using Raijin.SatSolver.Api.Endpoints;

namespace Raijin.SatSolver.Api;

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

    public static IApplicationBuilder MapEndpoints(this IApplicationBuilder app)
    {
        // Resolve all IEndpoint implementations and call their Map method
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        IEnumerable<IEndpoint> endpoints = scope.ServiceProvider.GetServices<IEndpoint>();

        foreach (IEndpoint endpoint in endpoints)
            endpoint.Map((WebApplication)app);

        return app;
    }
}