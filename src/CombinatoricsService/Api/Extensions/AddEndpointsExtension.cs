using Raijin.CombinatoricsService.Api.Endpoints;

namespace Raijin.CombinatoricsService.Api.Extensions;

public static class AddEndpointsExtension
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services)
    {
        IEnumerable<Type> endpointTypes = typeof(Program).Assembly.GetTypes()
            .Where(type =>
                typeof(IEndpoint).IsAssignableFrom(type) && type is { IsInterface: false, IsAbstract: false });

        foreach (Type endpointType in endpointTypes)
            services.AddSingleton(typeof(IEndpoint), endpointType);

        return services;
    }

    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        IEnumerable<IEndpoint> endpoints = endpointRouteBuilder.ServiceProvider.GetServices<IEndpoint>();

        foreach (IEndpoint endpoint in endpoints)
            endpoint.Map(endpointRouteBuilder);

        return endpointRouteBuilder;
    }
}