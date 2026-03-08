using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Infrastructure.Messaging;

namespace Raijin.CombinatoricsService.Infrastructure;

public static class InfrastructureModule
{
    public static Assembly Assembly => typeof(InfrastructureModule).Assembly;

    public static IServiceCollection AddInfrastructure(this IServiceCollection services) => services
        .AddMessaging();


    private static IServiceCollection AddMessaging(this IServiceCollection services) => services
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
}