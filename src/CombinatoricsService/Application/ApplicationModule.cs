using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Raijin.CombinatoricsService.Application.Factories;
using Raijin.CombinatoricsService.Application.Features.Problems.BooleanSatisfiability;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Messaging.Behaviors;
using Raijin.CombinatoricsService.Application.Parsing;
using Raijin.CombinatoricsService.Application.Reductions;
using Raijin.CombinatoricsService.Domain.BooleanExpressions;

namespace Raijin.CombinatoricsService.Application;

public static class ApplicationModule
{
    public static Assembly Assembly => typeof(ApplicationModule).Assembly;

    public static IServiceCollection AddApplication(this IServiceCollection services) => services
        .AddMessaging()
        .AddApplicationServices()
        .AddValidatorsFromAssembly(Assembly);

    private static IServiceCollection AddApplicationServices(this IServiceCollection services) => services
        .AddSingleton<BoolExprParser>()
        .AddSingleton<IBoolExprParser>(sp => sp.GetRequiredService<BoolExprParser>())
        .AddSingleton<IParser<BoolExpr>>(sp => sp.GetRequiredService<BoolExprParser>())
        .AddSingleton<IReduction<BoolExpr, TseitinResult>, TseitinReduction>()
        .AddScoped<IInstanceFactory, BooleanSatisfiabilityInstanceFactory>();

    private static IServiceCollection AddMessaging(this IServiceCollection services) => services
        .AddGenericInterfaceImplementations(typeof(IRequestHandler<>))
        .AddGenericInterfaceImplementations(typeof(IRequestHandler<,>))
        .AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
        .AddScoped(typeof(IPipelineBehavior<>), typeof(LoggingBehavior<>))
        .AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>))
        .AddScoped(typeof(IPipelineBehavior<>), typeof(ValidationBehavior<>));

    private static IServiceCollection AddGenericInterfaceImplementations(this IServiceCollection services,
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

        return services;
    }
}