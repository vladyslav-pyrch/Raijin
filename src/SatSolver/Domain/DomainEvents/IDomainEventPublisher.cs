namespace Raijin.SatSolver.Domain.DomainEvents;

public interface IDomainEventPublisher
{
    public Task Publish(IDomainEvent domainEvent, CancellationToken cancellationToken);
}