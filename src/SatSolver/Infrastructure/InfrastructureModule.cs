using System.Reflection;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raijin.SatSolver.Application.Messaging;
using Raijin.SatSolver.Application.Persistence;
using Raijin.SatSolver.Application.Solver;
using Raijin.SatSolver.Infrastructure.Messaging;
using Raijin.SatSolver.Infrastructure.Persistence;
using Raijin.SatSolver.Infrastructure.Persistence.Repositories;
using Raijin.SatSolver.Infrastructure.Solver;

namespace Raijin.SatSolver.Infrastructure;

public static class InfrastructureModule
{
    public static Assembly Assembly => typeof(InfrastructureModule).Assembly;

    public static IServiceCollection AddInfrastructureApi(this IServiceCollection services) => services
        .AddMessagingWithoutConsumers()
        .AddPersistence()
        .AddSatSolver();

    public static IServiceCollection AddInfrastructureWorker(this IServiceCollection services) => services
        .AddMessagingWithConsumers()
        .AddPersistence()
        .AddSatSolver();

    private static IServiceCollection AddMessagingWithoutConsumers(this IServiceCollection services) => services
        .AddMessagingCore()
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
        .AddMessagingCore()
        .AddMassTransit(x =>
        {
            x.AddConsumers(Assembly);
            x.SetKebabCaseEndpointNameFormatter();
            x.AddEntityFrameworkOutbox<SatSolverDbContext>();
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

    private static IServiceCollection AddMessagingCore(this IServiceCollection services) => services
        .AddScoped<IMediator, ServiceProviderMediator>()
        .AddSingleton<IMessageContextAccessor, AsyncLocalMessageContextAccessor>()
        .AddTransient<IMessageIdGenerator, GuidMessageIdGenerator>()
        .AddScoped<IMessageBus, MassTransitMessageBus>();

    public static IServiceCollection AddPersistence(this IServiceCollection services) => services
        .AddScoped<IUnitOfWork, SatSolverUnitOfWork>()
        .AddScoped<ISatProblemRepository, SatProblemRepository>()
        .AddDbContextPool<SatSolverDbContext>((provider, builder) =>
        {
            string connectionString = provider.GetRequiredService<IConfiguration>()
                                          .GetConnectionString("sat-solver-db") ??
                                      throw new InvalidOperationException(
                                          "Database connection string is not configured.");

            builder.UseNpgsql(connectionString, optionsBuilder =>
            {
                optionsBuilder.MigrationsAssembly(Assembly);
            });
        });

    private static IServiceCollection AddSatSolver(this IServiceCollection services) =>
        services.AddScoped<ISatSolver, CryptominisatSolver>();
}