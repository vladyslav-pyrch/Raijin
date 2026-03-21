using MassTransit;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Infrastructure.Messaging;

public sealed class MassTransitMessageBus(IPublishEndpoint publishEndpoint, ILogger<MassTransitMessageBus> logger)
    : IMessageBus
{
    public async Task Publish<TMessage>(object message, CancellationToken cancellationToken)
        where TMessage : class, IMessage
    {
        string messageType = typeof(TMessage).Name;
        logger.LogInformation("Publishing message {TMessage}: {@Message}", messageType, message);

        if (message.GetType().GetProperty("CorrelationId") is null)
            throw new InvalidOperationException("All messages should have \"CorrelationId\" set");

        await publishEndpoint.Publish<TMessage>(message, cancellationToken);
    }
}