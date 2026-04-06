using System.Reflection;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
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

    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
        Action<IBusRegistrationConfigurator>? busRegistrationConfiguration = null,
        Action<IServiceCollectionQuartzConfigurator>? serviceCollectionQuartzConfiguration = null,
        Action<IBusRegistrationContext, IBusFactoryConfigurator>? busFactoryConfiguration = null
    )
    {
        services.AddMessaging(busRegistrationConfiguration, busFactoryConfiguration);
        services.AddPersistence();
        services.AddSatSolver();
        services.AddQuartz(cfg => { serviceCollectionQuartzConfiguration?.Invoke(cfg); });

        return services;
    }

    private static IServiceCollection AddMessaging(this IServiceCollection services,
        Action<IBusRegistrationConfigurator>? busRegistrationConfiguration,
        Action<IBusRegistrationContext, IBusFactoryConfigurator>? busFactoryConfiguration
    )
    {
        services.AddScoped<IMediator, ServiceProviderMediator>();
        services.AddScoped<IMessageBus, MassTransitMessageBus>();
        services.AddSingleton<ICorrelationContextAccessor, CorrelationContextAccessor>();
        services.AddScoped(typeof(CorrelationContextConsumeFilter<>));
        services.AddScoped(typeof(CorrelationContextPublishFilter<>));
        services.AddScoped(typeof(LoggingConsumeFilter<>));
        services.AddScoped(typeof(LoggingPublishFilter<>));
        services.AddScoped(typeof(CausationConsumeFilter<>));
        services.AddScoped(typeof(CausationPublishFilter<>));
        services.AddMassTransit(x =>
        {
            busRegistrationConfiguration?.Invoke(x);

            x.AddEntityFrameworkOutbox<SatSolverDbContext>(configurator =>
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
                cfg.UseEntityFrameworkOutbox<SatSolverDbContext>(context, options =>
                {
                    options.MessageDeliveryLimit = 100;
                    options.MessageDeliveryTimeout = TimeSpan.FromSeconds(45);
                });
            });

            x.SetKebabCaseEndpointNameFormatter();
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri(GetRabbitMqConnectionString(context)));

                cfg.UseConsumeFilter(typeof(CorrelationContextConsumeFilter<>),
                    context);
                cfg.UseConsumeFilter(typeof(CausationConsumeFilter<>), context);
                cfg.UseConsumeFilter(typeof(LoggingConsumeFilter<>), context);

                cfg.UsePublishFilter(typeof(CorrelationContextPublishFilter<>),
                    context);
                cfg.UsePublishFilter(typeof(CausationPublishFilter<>), context);
                cfg.UsePublishFilter(typeof(LoggingPublishFilter<>), context);

                busFactoryConfiguration?.Invoke(context, cfg);

                cfg.UseInstrumentation();
            });
        });

        return services;
    }

    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, SatSolverUnitOfWork>();
        services.AddScoped<ISatProblemJobRepository, SatProblemRepository>();
        services.AddDbContext<SatSolverDbContext>((provider, builder) =>
        {
            builder.UseNpgsql(GetDatabaseConnectionString(provider),
                optionsBuilder => { optionsBuilder.MigrationsAssembly(Assembly); });
        });

        return services;
    }

    private static IServiceCollection AddSatSolver(this IServiceCollection services)
    {
        services.AddOptions<CryptominisatSolveOptions>()
            .Configure(options =>
            {
                string? binaryPath = Environment.GetEnvironmentVariable("CRYPTOMINISAT_FILE_NAME");
                if (!string.IsNullOrWhiteSpace(binaryPath))
                    options.FileName = binaryPath;
            });
        services.AddScoped<ISatSolver, CryptominisatSolver>();

        return services;
    }

    private static string GetRabbitMqConnectionString(IServiceProvider provider) =>
        provider.GetRequiredService<IConfiguration>().GetConnectionString("rabbit-mq")
        ?? throw new InvalidOperationException("RabbitMQ connection string is not configured.");

    private static string GetDatabaseConnectionString(IServiceProvider provider) =>
        provider.GetRequiredService<IConfiguration>().GetConnectionString("sat-solver-db")
        ?? throw new InvalidOperationException("Database connection string is not configured.");
}