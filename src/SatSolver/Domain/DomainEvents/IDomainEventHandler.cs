namespace Raijin.SatSolver.Domain.DomainEvents;

public interface IDomainEventHandler<in TDomainEvent> where TDomainEvent : IDomainEvent
{
    public Task Handle(TDomainEvent domainEvent, CancellationToken cancellationToken);
}