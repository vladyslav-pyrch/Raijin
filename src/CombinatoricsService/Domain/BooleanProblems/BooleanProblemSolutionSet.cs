using Raijin.CombinatoricsService.Domain.Abstractions;
using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.BooleanProblems;

public sealed record BooleanProblemSolutionSet(
    IReadOnlyList<VariableAssignment> Solution,
    Satisfiability Satisfiability
) : DomainEvent(DateTime.UtcNow);