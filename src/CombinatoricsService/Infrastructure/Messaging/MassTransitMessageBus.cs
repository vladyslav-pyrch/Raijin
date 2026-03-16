using MassTransit;
using Microsoft.Extensions.Logging;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Infrastructure.Messaging;

public class MassTransitMessageBus(IPublishEndpoint publishEndpoint, ILogger<MassTransitMessageBus> logger) : IMessageBus
{
    public async Task Publish<TMessage>(object message, CancellationToken cancellationToken) where TMessage : class
    {
        await publishEndpoint.Publish<TMessage>(message, cancellationToken);
    }
}