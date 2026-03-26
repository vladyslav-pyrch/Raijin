using Raijin.CombinatoricsService.Domain.Abstractions;

namespace Raijin.CombinatoricsService.Domain.BooleanProblems;

public sealed record BooleanProblemCreated(Guid Id, string Formula)
    : DomainEvent(DateTime.UtcNow);