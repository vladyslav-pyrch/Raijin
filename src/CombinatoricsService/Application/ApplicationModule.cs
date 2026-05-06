using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Messaging.Behaviors;
using Raijin.CombinatoricsService.Application.Parsing;

namespace Raijin.CombinatoricsService.Application;

public static class ApplicationModule
{
    public static Assembly Assembly => typeof(ApplicationModule).Assembly;

    public static void AddApplication(this IServiceCollection services)
    {
        services.AddMessaging();
        services.AddParsers();
        services.AddValidatorsFromAssembly(Assembly);
    }

    public static void AddParsers(this IServiceCollection services)
    {
        services.AddSingleton<IBoolExprParser, BoolExprParser>();
    }

    private static void AddMessaging(this IServiceCollection services)
    {
        services.AddGenericInterfaceImplementations(typeof(IRequestHandler<>));
        services.AddGenericInterfaceImplementations(typeof(IRequestHandler<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<>), typeof(LoggingBehavior<>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<>), typeof(ValidationBehavior<>));
    }

    private static void AddGenericInterfaceImplementations(this IServiceCollection services,
        Type genericInterfaceType)
    {
        IEnumerable<Type> types = Assembly.GetTypes()
            .Where(t => t.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericInterfaceType) && !t.IsAbstract);

        foreach (Type type in types)
        {
            Type[] genericArguments = type.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericInterfaceType)
                .GetGenericArguments();

            services.AddScoped(genericInterfaceType.MakeGenericType(genericArguments), type);
        }
    }
}