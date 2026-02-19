using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Raijin.SatSolver.Application.Events;

namespace Raijin.SatSolver.Infrastructure.Messaging;

internal sealed class RabbitMqSatProblemGeneratedConsumer(
    IConnectionFactory connectionFactory,
    IServiceProvider provider,
    IOptions<RabbitMqOptions> options) : IConsumer
{
    private readonly RabbitMqOptions _options = options.Value;

    private static readonly string RoutingKey = SatProblemGenerated.GetMetadata().Name;

    private string QueueName => $"{_options.QueuePrefix}.{RoutingKey}.queue";

    public async Task Start(CancellationToken cancellationToken)
    {
        await using IConnection connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        await using IChannel channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: _options.Exchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: QueueName,
            exchange: _options.Exchange,
            routingKey: RoutingKey,
            cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += OnConsumerOnReceivedAsync;

        await channel.BasicConsumeAsync(QueueName, false, consumer, cancellationToken);

        await Task.Delay(Timeout.Infinite, cancellationToken);

        return;

        async Task OnConsumerOnReceivedAsync(object _, BasicDeliverEventArgs eventArgs)
        {
            byte[] body = eventArgs.Body.ToArray();
            string messageJson = Encoding.UTF8.GetString(body);

            SatProblemGenerated @event = JsonSerializer.Deserialize<SatProblemGenerated>(messageJson) ??
                                         throw new InvalidOperationException("Failed to deserialize event");

            using IServiceScope scope = provider.CreateScope();

            var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<SatProblemGenerated>>();
            await handler.Handle(@event, cancellationToken);

            await channel.BasicAckAsync(eventArgs.DeliveryTag, false, cancellationToken);
        }
    }
}