using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Raijin.SatSolver.Application.Messaging;
using Raijin.SatSolver.Application.Messaging.Behaviors;

namespace Raijin.SatSolver.Application;

public static class ApplicationModule
{
    public static readonly Assembly Assembly = typeof(ApplicationModule).Assembly;

    public static IServiceCollection AddApplication(this IServiceCollection services) => services
        .AddCommandHandlers()
        .AddEventHandlers()
        .AddPipelineBehaviors()
        .AddValidatorsFromAssembly(Assembly);

    private static IServiceCollection AddCommandHandlers(this IServiceCollection services) => services
        .AddGenericInterfaceImplementations(typeof(IRequestHandler<>))
        .AddGenericInterfaceImplementations(typeof(IRequestHandler<,>));

    private static IServiceCollection AddEventHandlers(this IServiceCollection services) => services
        .AddGenericInterfaceImplementations(typeof(IMessageHandler<>));
    
    private static IServiceCollection AddPipelineBehaviors(this IServiceCollection services) => services
        .AddScoped(typeof(IPipelineBehavior<,>), typeof(ContextBehavior<,>))
        .AddScoped(typeof(IPipelineBehavior<>), typeof(ContextBehavior<>))
        .AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
        .AddScoped(typeof(IPipelineBehavior<>), typeof(LoggingBehavior<>))
        .AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>))
        .AddScoped(typeof(IPipelineBehavior<>), typeof(ValidationBehavior<>));

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