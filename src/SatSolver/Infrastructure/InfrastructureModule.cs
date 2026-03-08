using System.Reflection;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Raijin.SatSolver.Application.Messaging;
using Raijin.SatSolver.Application.Persistence;
using Raijin.SatSolver.Application.Solver;
using Raijin.SatSolver.Domain.DomainEvents;
using Raijin.SatSolver.Infrastructure.DomainEvents;
using Raijin.SatSolver.Infrastructure.Messaging;
using Raijin.SatSolver.Infrastructure.Persistence;
using Raijin.SatSolver.Infrastructure.Persistence.Repositories;
using Raijin.SatSolver.Infrastructure.Solver;

namespace Raijin.SatSolver.Infrastructure;

public static class InfrastructureModule
{
    public static Assembly Assembly => typeof(InfrastructureModule).Assembly;

    public static async Task ApplyMigrations(this IHost host)
    {
        using IServiceScope scope = host.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<SatSolverDbContext>>();
        var satSolverDbContext = scope.ServiceProvider.GetRequiredService<SatSolverDbContext>();

        logger.LogInformation("Applying database migrations...");

        await satSolverDbContext.Database.MigrateAsync();

        logger.LogInformation("Database migrations applied successfully.");
    }

    public static IServiceCollection AddInfrastructureApi(this IServiceCollection services) => services
        .AddDomainEvents()
        .AddMessagingCore()
        .AddPersistence()
        .AddSatSolver();

    public static IServiceCollection AddInfrastructureWorker(this IServiceCollection services) => services
        .AddDomainEvents()
        .AddMessagingWithConsumers()
        .AddPersistence()
        .AddSatSolver();

    private static IServiceCollection AddDomainEvents(this IServiceCollection services) =>
        services.AddScoped<IDomainEventPublisher, DotNetDiDomainEventPublisher>();

    private static IServiceCollection AddMessagingCore(this IServiceCollection services) => services
        .AddScoped<IMediator, ServiceProviderMediator>()
        .AddScoped<IMessageBus, MassTransitMessageBus>()
        .AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.UsingRabbitMq((context, cfg) =>
            {
                string connectionString = context.GetRequiredService<IConfiguration>()
                                              .GetConnectionString("rabbit-mq") ??
                                          throw new InvalidOperationException(
                                              "RabbitMQ connection string is not configured.");

                cfg.Host(new Uri(connectionString));
            });
        });

    private static IServiceCollection AddMessagingWithConsumers(this IServiceCollection services) => services
        .AddScoped<IMediator, ServiceProviderMediator>()
        .AddScoped<IMessageBus, MassTransitMessageBus>()
        .AddMassTransit(x =>
        {
            x.AddConsumers(Assembly);
            x.SetKebabCaseEndpointNameFormatter();
            x.UsingRabbitMq((context, cfg) =>
            {
                string connectionString = context.GetRequiredService<IConfiguration>()
                                              .GetConnectionString("rabbit-mq") ??
                                          throw new InvalidOperationException(
                                              "RabbitMQ connection string is not configured.");

                cfg.Host(new Uri(connectionString));
                cfg.ConfigureEndpoints(context);
            });
        });

    private static IServiceCollection AddPersistence(this IServiceCollection services) => services
        .AddScoped<ISatProblemRepository, SatProblemRepository>()
        .AddDbContextPool<SatSolverDbContext>((provider, builder) =>
        {
            string connectionString = provider.GetRequiredService<IConfiguration>()
                                          .GetConnectionString("sat-solver-db") ??
                                      throw new InvalidOperationException(
                                          "Database connection string is not configured.");

            builder.UseNpgsql(connectionString);
        });

    private static IServiceCollection AddSatSolver(this IServiceCollection services) =>
        services.AddScoped<ISatSolver, CryptominisatSolver>();
}