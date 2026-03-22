using System.Reflection;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raijin.Application.Contracts;
using Raijin.SatSolver.Application.Messaging;
using Raijin.SatSolver.Application.Persistence;
using Raijin.SatSolver.Application.Solver;
using Raijin.SatSolver.Infrastructure.Messaging;
using Raijin.SatSolver.Infrastructure.Messaging.Filters;
using Raijin.SatSolver.Infrastructure.Persistence;
using Raijin.SatSolver.Infrastructure.Persistence.Repositories;
using Raijin.SatSolver.Infrastructure.Solver;

namespace Raijin.SatSolver.Infrastructure;

public static class InfrastructureModule
{
    public static Assembly Assembly => typeof(InfrastructureModule).Assembly;

    public static IServiceCollection AddInfrastructureApi(this IServiceCollection services)
    {
        return services
            .AddMessagingWithoutConsumers()
            .AddPersistence()
            .AddSatSolver();
    }

    public static IServiceCollection AddInfrastructureWorker(this IServiceCollection services)
    {
        return services
            .AddMessagingWithConsumers()
            .AddPersistence()
            .AddSatSolver();
    }

    private static IServiceCollection AddMessagingWithoutConsumers(this IServiceCollection services)
    {
        return services
            .AddMessagingCore()
            .AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(new Uri(GetRabbitMqConnectionString(context)));

                    cfg.UsePublishFilter(typeof(CorrelationContextPublishFilter<>), context);
                    cfg.UsePublishFilter(typeof(CausationPublishFilter<>), context);
                    cfg.UsePublishFilter(typeof(LoggingPublishFilter<>), context);

                    cfg.UseInstrumentation();
                });
            });
    }

    private static IServiceCollection AddMessagingWithConsumers(this IServiceCollection services)
    {
        return services
            .AddMessagingCore()
            .AddMassTransit(x =>
            {
                x.AddConsumer<MessageConsumer<ISatProblemSubmitted>>();
                x.AddConsumer<MessageConsumer<ISatProblemSent>>();
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

    private static IServiceCollection AddMessagingCore(this IServiceCollection services)
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
            .AddScoped(typeof(CausationPublishFilter<>));
    }

    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        return services
            .AddScoped<IUnitOfWork, SatSolverUnitOfWork>()
            .AddScoped<ISatProblemRepository, SatProblemRepository>()
            .AddDbContext<SatSolverDbContext>((provider, builder) =>
            {
                builder.UseNpgsql(GetDatabaseConnectionString(provider),
                    optionsBuilder => { optionsBuilder.MigrationsAssembly(Assembly); });
            });
    }

    private static IServiceCollection AddSatSolver(this IServiceCollection services)
    {
        return services
            .AddOptions<CryptominisatSolveOptions>()
            .Configure(options =>
            {
                string? binaryPath = Environment.GetEnvironmentVariable("CRYPTOMINISAT_FILE_NAME");
                if (!string.IsNullOrWhiteSpace(binaryPath))
                    options.FileName = binaryPath;
            }).Services
            .AddScoped<ISatSolver, CryptominisatSolver>();
    }

    private static string GetRabbitMqConnectionString(IServiceProvider provider)
    {
        return provider.GetRequiredService<IConfiguration>().GetConnectionString("rabbit-mq")
               ?? throw new InvalidOperationException("RabbitMQ connection string is not configured.");
    }

    private static string GetDatabaseConnectionString(IServiceProvider provider)
    {
        return provider.GetRequiredService<IConfiguration>().GetConnectionString("sat-solver-db")
               ?? throw new InvalidOperationException("Database connection string is not configured.");
    }
}