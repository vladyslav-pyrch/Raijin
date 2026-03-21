using System.Reflection;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raijin.Application.Contracts;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Infrastructure.Messaging;
using Raijin.CombinatoricsService.Infrastructure.Persistence;
using Raijin.CombinatoricsService.Infrastructure.Persistence.Repositories;

namespace Raijin.CombinatoricsService.Infrastructure;

public static class InfrastructureModule
{
    public static Assembly Assembly => typeof(InfrastructureModule).Assembly;

    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        return services
            .AddMessaging()
            .AddPersistence();
    }


    private static IServiceCollection AddMessaging(this IServiceCollection services)
    {
        return services
            .AddScoped<IMediator, ServiceProviderMediator>()
            .AddScoped<IMessageBus, MassTransitMessageBus>()
            .AddSingleton<ICorrelationContextAccessor, CorrelationContextAccessor>()
            .AddScoped(typeof(CorrelationContextConsumeFilter<>))
            .AddMassTransit(x =>
            {
                x.AddConsumer<MassTransitMessageConsumer<ICombinatoricProblemSubmitted>>();
                x.AddConsumer<MassTransitMessageConsumer<IBooleanProblemSubmitted>>();
                x.AddConsumer<MassTransitMessageConsumer<ISatProblemSolved>>();
                x.AddConsumer<MassTransitMessageConsumer<IBooleanProblemSolved>>();
                x.SetKebabCaseEndpointNameFormatter();
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(new Uri(GetRabbitMqConnectionString(context)));
                    cfg.UseConsumeFilter(typeof(CorrelationContextConsumeFilter<>), context);
                    cfg.ConfigureEndpoints(context);
                    cfg.UseInstrumentation();
                });
            });
    }

    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        return services
            .AddScoped<IUnitOfWork, CombinatoricsServiceUnitOfWork>()
            .AddScoped<ICombinatoricProblemRepository, CombinatoricProblemRepository>()
            .AddScoped<IBooleanProblemRepository, BooleanProblemRepository>()
            .AddDbContext<CombinatoricsServiceDbContext>((provider, builder) =>
            {
                builder.UseNpgsql(GetDatabaseConnectionString(provider),
                    optionsBuilder => { optionsBuilder.MigrationsAssembly(Assembly); });
            });
    }

    private static string GetRabbitMqConnectionString(IServiceProvider provider)
    {
        return provider.GetRequiredService<IConfiguration>().GetConnectionString("rabbit-mq")
               ?? throw new InvalidOperationException("RabbitMQ connection string is not configured.");
    }

    private static string GetDatabaseConnectionString(IServiceProvider provider)
    {
        return provider.GetRequiredService<IConfiguration>().GetConnectionString("combinatorics-service-db")
               ?? throw new InvalidOperationException("Database connection string is not configured.");
    }
}