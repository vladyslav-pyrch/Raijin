using Raijin.CombinatoricsService.Domain.Abstractions;
using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.CombinatoricProblems;

public sealed record CombinatoricProblemSolutionSet(
    IReadOnlyList<DecisionVariableAssignment> Solution,
    Satisfiability Satisfiability)
    : DomainEvent(DateTime.UtcNow);