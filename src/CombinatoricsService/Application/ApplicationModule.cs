using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Application;

public static class ApplicationModule
{
    public static Assembly Assembly => typeof(ApplicationModule).Assembly;

    public static IServiceCollection AddApplication(this IServiceCollection services) => services
        .AddCommandHandlers()
        .AddEventHandlers()
        .AddValidatorsFromAssembly(Assembly);

    private static IServiceCollection AddCommandHandlers(this IServiceCollection services) => services
        .AddGenericInterfaceImplementations(typeof(ICommandHandler<>))
        .AddGenericInterfaceImplementations(typeof(ICommandHandler<,>));

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
