using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Raijin.SatSolver.Application.Abstractions;
using Raijin.SatSolver.Infrastructure.Cqrs;
using Raijin.SatSolver.Infrastructure.Messaging;
using Raijin.SatSolver.Infrastructure.Persistence;
using Raijin.SatSolver.Infrastructure.Persistence.Repositories;
using Raijin.SatSolver.Infrastructure.Solver;

namespace Raijin.SatSolver.Infrastructure;

public static class InfrastructureModule
{
    public static Assembly Assembly => typeof(InfrastructureModule).Assembly;

    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IMediator, DotNetDiMediator>();
        services.AddScoped<ISatSolver, CryptominisatSolver>();
        services.AddScoped<ISatProblemRepository, SatProblemRepository>();
        services.AddDbContextPool<SatSolverDbContext>((provider, builder) =>
        {
            string connectionString = provider.GetRequiredService<IConfiguration>()
                                          .GetConnectionString("sat-solver-db") ??
                                      throw new InvalidOperationException(
                                          "Database connection string is not configured.");

            builder.UseNpgsql(connectionString);
        });
        services.AddSingleton<IConnectionFactory>(provider =>
        {
            string connectionString = provider.GetRequiredService<IConfiguration>()
                                          .GetConnectionString("rabbit-mq") ??
                                      throw new InvalidOperationException(
                                          "RabbitMQ connection string is not configured.");

            return new ConnectionFactory
            {
                Uri = new Uri(connectionString)
            };
        });
        services.AddOptions<RabbitMqOptions>()
            .BindConfiguration("RABBIT_MQ")
            .ValidateDataAnnotations();
        services.AddSingleton<IEventBus, RabbitMqEventBus>();
        services.AddInterfaceImplementations<IConsumer>();

        return services;
    }

    private static IServiceCollection AddInterfaceImplementations<TInterface>(this IServiceCollection services) where TInterface : class
    {
        IEnumerable<Type> handlerTypes = Assembly.GetTypes()
            .Where(t => typeof(TInterface).IsAssignableFrom(t) && !t.IsAbstract);

        foreach (Type handlerType in handlerTypes)
            services.AddScoped(typeof(TInterface), handlerType);

        return services;
    }
}
