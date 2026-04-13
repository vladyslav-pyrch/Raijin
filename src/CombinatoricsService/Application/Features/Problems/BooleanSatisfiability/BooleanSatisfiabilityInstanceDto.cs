using System.Text.Json.Serialization;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Application.Features.Problems.BooleanSatisfiability;

public record BooleanSatisfiabilityInstanceDto(IEnumerable<IEnumerable<string>> Clauses)
    : InstanceDto
{
    [JsonIgnore] public override string ProblemType => ProblemTypes.BooleanSatisfiabilityProblem;
}