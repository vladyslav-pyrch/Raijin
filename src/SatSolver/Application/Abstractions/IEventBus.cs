using Raijin.SatSolver.Application.Events;

namespace Raijin.SatSolver.Application.Abstractions;

public interface IEventBus
{
    public Task Publish<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent;


}