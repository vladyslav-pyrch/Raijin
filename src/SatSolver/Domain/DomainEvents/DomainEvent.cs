namespace Raijin.SatSolver.Domain.DomainEvents;

public abstract record DomainEvent(Guid Id, DateTime CreatedAt) : IDomainEvent;