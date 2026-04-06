using System.Reflection;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
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
        services.AddMessaging();
        services.AddPersistence();

        return services;
    }

    private static IServiceCollection AddMessaging(this IServiceCollection services)
    {
        services.AddScoped<IMediator, ServiceProviderMediator>();
        services.AddScoped<IMessageBus, MassTransitMessageBus>();
        services.AddSingleton<ICorrelationContextAccessor, CorrelationContextAccessor>();
        services.AddScoped(typeof(CorrelationContextConsumeFilter<>));
        services.AddScoped(typeof(CorrelationContextPublishFilter<>));
        services.AddScoped(typeof(CorrelationContextSendFilter<>));
        services.AddScoped(typeof(LoggingConsumeFilter<>));
        services.AddScoped(typeof(LoggingPublishFilter<>));
        services.AddScoped(typeof(LoggingSendFilter<>));
        services.AddScoped(typeof(CausationConsumeFilter<>));
        services.AddScoped(typeof(CausationPublishFilter<>));
        services.AddScoped(typeof(CausationSendFilter<>));
        services.AddMassTransit(x =>
        {
            x.AddEntityFrameworkOutbox<CombinatoricsServiceDbContext>(configurator =>
            {
                configurator.UsePostgres();

                configurator.UseBusOutbox(options =>
                {
                    options.MessageDeliveryLimit = 100;
                    options.MessageDeliveryTimeout = TimeSpan.FromSeconds(45);
                });
            });
            x.AddConfigureEndpointsCallback((context, name, cfg) =>
            {
                cfg.UseEntityFrameworkOutbox<CombinatoricsServiceDbContext>(context, options =>
                {
                    options.MessageDeliveryLimit = 100;
                    options.MessageDeliveryTimeout = TimeSpan.FromSeconds(45);
                });
            });

            x.SetKebabCaseEndpointNameFormatter();
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri(GetRabbitMqConnectionString(context)));

                cfg.UseConsumeFilter(typeof(CorrelationContextConsumeFilter<>), context);
                cfg.UseConsumeFilter(typeof(CausationConsumeFilter<>), context);
                cfg.UseConsumeFilter(typeof(LoggingConsumeFilter<>), context);

                cfg.UseSendFilter(typeof(CorrelationContextSendFilter<>), context);
                cfg.UseSendFilter(typeof(CausationSendFilter<>), context);
                cfg.UseSendFilter(typeof(LoggingSendFilter<>), context);

                cfg.UsePublishFilter(typeof(CorrelationContextPublishFilter<>), context);
                cfg.UsePublishFilter(typeof(CausationPublishFilter<>), context);
                cfg.UsePublishFilter(typeof(LoggingPublishFilter<>), context);

                cfg.ConfigureEndpoints(context);
                cfg.UseInstrumentation();
            });
        });

        return services;
    }

    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddDbContextPool<CombinatoricsServiceDbContext>((provider, builder) =>
        {
            NpgsqlDataSource dataSource = new NpgsqlDataSourceBuilder(GetDatabaseConnectionString(provider))
                .Build();

            builder.UseNpgsql(dataSource, optionsBuilder => optionsBuilder.MigrationsAssembly(Assembly));
        });
        services.AddScoped<IUnitOfWork, CombinatoricsServiceUnitOfWork>();
        services.AddScoped<IProblemRepository, ProblemRepository>();

        return services;
    }

    private static string GetRabbitMqConnectionString(IServiceProvider provider) =>
        provider.GetRequiredService<IConfiguration>().GetConnectionString("rabbit-mq")
        ?? throw new InvalidOperationException("RabbitMQ connection string is not configured.");

    private static string GetDatabaseConnectionString(IServiceProvider provider) =>
        provider.GetRequiredService<IConfiguration>().GetConnectionString("combinatorics-service-db")
        ?? throw new InvalidOperationException("Database connection string is not configured.");
}