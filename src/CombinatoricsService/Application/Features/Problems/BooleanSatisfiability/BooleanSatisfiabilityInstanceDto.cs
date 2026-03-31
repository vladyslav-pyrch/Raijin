namespace Raijin.CombinatoricsService.Application.Features.Problems.BooleanSatisfiability;

public record BooleanSatisfiabilityInstanceDto(IEnumerable<IEnumerable<int>> Clauses)
    : InstanceDto;