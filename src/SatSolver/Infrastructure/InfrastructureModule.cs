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
            x.AddEntityFrameworkOutbox<SatSolverDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            });
            x.UsingRabbitMq((context, cfg) => { cfg.Host(new Uri(GetRabbitMqConnectionString(context))); });
        });

    private static IServiceCollection AddMessagingWithConsumers(this IServiceCollection services) => services
        .AddMessagingCore()
        .AddMassTransit(x =>
        {
            x.AddConsumers(Assembly);
            x.SetKebabCaseEndpointNameFormatter();
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri(GetRabbitMqConnectionString(context)));
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
        .AddDbContext<SatSolverDbContext>((provider, builder) =>
        {
            builder.UseNpgsql(GetDatabaseConnectionString(provider),
                optionsBuilder => { optionsBuilder.MigrationsAssembly(Assembly); });
        });

    private static IServiceCollection AddSatSolver(this IServiceCollection services) => services
        .AddOptions<CryptominisatSolveOptions>()
        .Configure(options =>
        {
            string? binaryPath = Environment.GetEnvironmentVariable("CRYPTOMINISAT_FILE_NAME");
            if (!string.IsNullOrWhiteSpace(binaryPath))
                options.FileName = binaryPath;
        }).Services
        .AddScoped<ISatSolver, CryptominisatSolver>();

    private static string GetRabbitMqConnectionString(IServiceProvider provider) =>
        provider.GetRequiredService<IConfiguration>().GetConnectionString("rabbit-mq")
        ?? throw new InvalidOperationException("RabbitMQ connection string is not configured.");

    private static string GetDatabaseConnectionString(IServiceProvider provider) =>
        provider.GetRequiredService<IConfiguration>().GetConnectionString("sat-solver-db")
        ?? throw new InvalidOperationException("Database connection string is not configured.");
}