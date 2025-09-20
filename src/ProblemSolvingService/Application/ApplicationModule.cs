using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Raijin.ProblemSolvingService.Application.Cqrs;

namespace Raijin.ProblemSolvingService.Application;

public static class ApplicationModule
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddRequestHandlers();
        return services;
    }

    private static IServiceCollection AddRequestHandlers(this IServiceCollection services)
    {
        List<Type> handlers = typeof(IRequestHandler<,>).GetImplementations();

        foreach (Type handler in handlers)
        {
            Type interfaceType = handler.GetGenericInterface(typeof(IRequestHandler<,>));
            services.AddTransient(interfaceType, handler);
        }

        return services;
    }

    private static List<Type> GetImplementations(this Type interfaceType) =>
        Assembly.GetAssembly(typeof(ApplicationModule))!
            .GetTypes()
            .Where(t =>
                t is { IsClass: true, IsAbstract: false } && t.ImplementsInterface(typeof(IRequestHandler<,>)))
            .ToList();

    private static bool ImplementsInterface(this Type type, Type interfaceType) =>
        type.GetInterfaces().Any(i =>
            i == interfaceType || (i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType));

    private static Type GetGenericInterface(this Type type, Type genericInterfaceType) =>
        type.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericInterfaceType);
}