using Raijin.CombinatoricsService.Domain.Abstractions;

namespace Raijin.CombinatoricsService.Domain.CombinatoricProblems;

public sealed record ConstraintAdded(Constraint Constraint)
    : DomainEvent(DateTime.UtcNow);