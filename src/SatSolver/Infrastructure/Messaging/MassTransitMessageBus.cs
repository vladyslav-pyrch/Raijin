using MassTransit;
using Raijin.SatSolver.Application.Messaging;

namespace Raijin.SatSolver.Infrastructure.Messaging;

public class MassTransitMessageBus(IPublishEndpoint publishEndpoint) : IMessageBus
{
    public async Task Publish<TMessage>(object message, CancellationToken cancellationToken) where TMessage : class
    {
        await publishEndpoint.Publish<TMessage>(message, cancellationToken);
    }
}