namespace Raijin.SatSolver.Application.Events;

public interface IEventHandler<in TEvent> where TEvent : IEvent
{
    public Task Handle(TEvent @event, CancellationToken cancellationToken);
}