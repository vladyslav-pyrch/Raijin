using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Raijin.SatSolver.Application.Cqrs;

namespace Raijin.SatSolver.Application;

public static class ApplicationModule
{
    public static readonly Assembly Assembly = typeof(ApplicationModule).Assembly;

    public static IServiceCollection AddApplication(this IServiceCollection services) => services
        .AddRequestHandlers()
        .AddValidatorsFromAssembly(Assembly);

    private static IServiceCollection AddRequestHandlers(this IServiceCollection services) => services
        .AddGenericInterfaceImplementations(typeof(IRequestHandler<>))
        .AddGenericInterfaceImplementations(typeof(IRequestHandler<,>));

    private static IServiceCollection AddGenericInterfaceImplementations(this IServiceCollection services, Type genericInterfaceType)
    {
        IEnumerable<Type> handlerTypes = Assembly.GetTypes()
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericInterfaceType) && !t.IsAbstract);

        foreach (Type handlerType in handlerTypes)
        {
            Type[] genericArguments = handlerType.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericInterfaceType)
                .GetGenericArguments();

            services.AddScoped(genericInterfaceType.MakeGenericType(genericArguments), handlerType);
        }

        return services;
    }
}