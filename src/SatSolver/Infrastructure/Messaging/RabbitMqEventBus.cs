using Raijin.SatSolver.Application.Abstractions;
using Raijin.SatSolver.Application.Events;

namespace Raijin.SatSolver.Infrastructure.Messaging;

public class RabbitMqEventBus : IEventBus
{
    public Task Publish(IEvent @event, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}