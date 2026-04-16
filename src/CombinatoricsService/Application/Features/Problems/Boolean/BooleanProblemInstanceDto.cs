using System.Text.Json.Serialization;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Application.Features.Problems.Boolean;

public record BooleanProblemInstanceDto(string Formula) : InstanceDto
{
    [JsonIgnore] public override string ProblemType => ProblemTypes.BooleanProblem;
}
