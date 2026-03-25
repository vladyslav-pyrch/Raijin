using System.Reflection;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Raijin.Application.Contracts;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Infrastructure.Messaging;
using Raijin.CombinatoricsService.Infrastructure.Messaging.Filters;
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
            .AddScoped(typeof(CorrelationContextPublishFilter<>))
            .AddScoped(typeof(LoggingConsumeFilter<>))
            .AddScoped(typeof(LoggingPublishFilter<>))
            .AddScoped(typeof(CausationConsumeFilter<>))
            .AddScoped(typeof(CausationPublishFilter<>))
            .AddMassTransit(x =>
            {
                x.AddConsumer<MessageConsumer<ICombinatoricProblemSubmitted>>();
                x.AddConsumer<MessageConsumer<ISatProblemSolved>>();
                x.AddConsumer<MessageConsumer<IBooleanProblemSolved>>();
                x.SetKebabCaseEndpointNameFormatter();
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(new Uri(GetRabbitMqConnectionString(context)));

                    cfg.UseConsumeFilter(typeof(CorrelationContextConsumeFilter<>), context);
                    cfg.UseConsumeFilter(typeof(CausationConsumeFilter<>), context);
                    cfg.UseConsumeFilter(typeof(LoggingConsumeFilter<>), context);

                    cfg.UsePublishFilter(typeof(CorrelationContextPublishFilter<>), context);
                    cfg.UsePublishFilter(typeof(CausationPublishFilter<>), context);
                    cfg.UsePublishFilter(typeof(LoggingPublishFilter<>), context);

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
                NpgsqlDataSource dataSource = new NpgsqlDataSourceBuilder(GetDatabaseConnectionString(provider))
                    .EnableDynamicJson()
                    .Build();

                builder.UseNpgsql(dataSource, optionsBuilder => optionsBuilder.MigrationsAssembly(Assembly));
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