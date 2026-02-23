using Raijin.SatSolver.Domain.DomainEvents;

namespace Raijin.SatSolver.Domain.SatProblems;

public record SatProblemCreatedDomainEvent(string Dimacs) : DomainEvent(Id: Guid.CreateVersion7(), CreatedAt: DateTime.UtcNow);