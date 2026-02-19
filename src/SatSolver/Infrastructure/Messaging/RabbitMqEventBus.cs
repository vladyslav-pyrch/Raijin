using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Raijin.SatSolver.Application.Abstractions;
using Raijin.SatSolver.Application.Events;

namespace Raijin.SatSolver.Infrastructure.Messaging;

public class RabbitMqEventBus(IConnectionFactory connectionFactory, IOptions<RabbitMqOptions> options) : IEventBus, IDisposable, IAsyncDisposable
{
    private readonly RabbitMqOptions _options = options.Value;

    private IConnection? _connection;

    public async Task Publish<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent
    {
        await using IChannel channel = await CreateChannelAsync(cancellationToken);
        await channel.ExchangeDeclareAsync(
            exchange: _options.Exchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        byte[] body = SerializeEvent(@event);
        string routingKey = @event.Metadata.Name;

        await channel.BasicPublishAsync(
            exchange: _options.Exchange,
            routingKey: routingKey,
            mandatory: true,
            body: body,
            cancellationToken: cancellationToken);
    }

    private async ValueTask<IChannel> CreateChannelAsync(CancellationToken cancellationToken)
    {
        IConnection connection = await GetConnectionAsync(cancellationToken);
        return await connection.CreateChannelAsync(cancellationToken: cancellationToken);
    }

    private async ValueTask<IConnection> GetConnectionAsync(CancellationToken cancellationToken)
    {
        if (_connection is { IsOpen: true })
            return _connection;

        _connection = await connectionFactory.CreateConnectionAsync(cancellationToken);

        return _connection;
    }

    private static byte[] SerializeEvent<TEvent>(TEvent @event)
    {
        string json = JsonSerializer.Serialize(@event);
        return Encoding.UTF8.GetBytes(json);
    }

    void IDisposable.Dispose()
    {
        _connection?.Dispose();
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }
    }
}