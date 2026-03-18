using System.Reflection;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Infrastructure.Messaging;
using Raijin.CombinatoricsService.Infrastructure.Persistence;
using Raijin.CombinatoricsService.Infrastructure.Persistence.Repositories;

namespace Raijin.CombinatoricsService.Infrastructure;

public static class InfrastructureModule
{
    public static Assembly Assembly => typeof(InfrastructureModule).Assembly;

    public static IServiceCollection AddInfrastructure(this IServiceCollection services) => services
        .AddMessaging()
        .AddPersistence();


    private static IServiceCollection AddMessaging(this IServiceCollection services) => services
        .AddScoped<IMediator, ServiceProviderMediator>()
        .AddSingleton<IMessageContextAccessor, AsyncLocalMessageContextAccessor>()
        .AddTransient<IMessageIdGenerator, GuidMessageIdGenerator>()
        .AddScoped<IMessageBus, MassTransitMessageBus>()
        .AddMassTransit(x =>
        {
            x.AddConsumers(Assembly);
            x.SetKebabCaseEndpointNameFormatter();
            x.AddEntityFrameworkOutbox<CombinatoricsServiceDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            });
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

    public static IServiceCollection AddPersistence(this IServiceCollection services) => services
        .AddScoped<IUnitOfWork, CombinatoricsServiceUnitOfWork>()
        .AddScoped<ICombinatoricProblemRepository, CombinatoricProblemRepository>()
        .AddScoped<DbContextResolver>()
        .AddDbContextFactory<CombinatoricsServiceDbContext>((provider, builder) =>
        {
            string connectionString = provider.GetRequiredService<IConfiguration>()
                                          .GetConnectionString("combinatorics-service-db") ??
                                      throw new InvalidOperationException(
                                          "Database connection string is not configured.");

            builder.UseNpgsql(connectionString, optionsBuilder =>
            {
                optionsBuilder.MigrationsAssembly(Assembly);
            });
        })
        .AddDbContext<CombinatoricsServiceDbContext>((provider, builder) =>
        {
            string connectionString = provider.GetRequiredService<IConfiguration>()
                                          .GetConnectionString("combinatorics-service-db") ??
                                      throw new InvalidOperationException(
                                          "Database connection string is not configured.");

            builder.UseNpgsql(connectionString, optionsBuilder =>
            {
                optionsBuilder.MigrationsAssembly(Assembly);
            });
        });

}
