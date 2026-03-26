using Raijin.CombinatoricsService.Domain.Abstractions;

namespace Raijin.CombinatoricsService.Domain.CombinatoricProblems;

public sealed record DecisionVariableAdded(DecisionVariable DecisionVariable)
    : DomainEvent(DateTime.UtcNow);