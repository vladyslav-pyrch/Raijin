using Raijin.SatSolver.Application.Events;

namespace Raijin.SatSolver.Application.Abstractions;

public interface IEventBus
{
    public Task Publish(IEvent @event, CancellationToken cancellationToken);


}