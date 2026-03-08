using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Raijin.SatSolver.Application.Messaging;

namespace Raijin.SatSolver.Application;

public static class ApplicationModule
{
    public static readonly Assembly Assembly = typeof(ApplicationModule).Assembly;

    public static IServiceCollection AddApplication(this IServiceCollection services) => services
        .AddRequestHandlers()
        .AddEventHandlers()
        .AddValidatorsFromAssembly(Assembly);

    private static IServiceCollection AddRequestHandlers(this IServiceCollection services) => services
        .AddGenericInterfaceImplementations(typeof(IRequestHandler<>))
        .AddGenericInterfaceImplementations(typeof(IRequestHandler<,>));

    private static IServiceCollection AddEventHandlers(this IServiceCollection services) => services
        .AddGenericInterfaceImplementations(typeof(IMessageHandler<>));

    private static IServiceCollection AddGenericInterfaceImplementations(this IServiceCollection services, Type genericInterfaceType)
    {
        IEnumerable<Type> types = Assembly.GetTypes()
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericInterfaceType) && !t.IsAbstract);

        foreach (Type type in types)
        {
            Type[] genericArguments = type.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericInterfaceType)
                .GetGenericArguments();

            services.AddScoped(genericInterfaceType.MakeGenericType(genericArguments), type);
        }

        return services;
    }
}