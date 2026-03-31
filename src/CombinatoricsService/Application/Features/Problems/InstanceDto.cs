using System.Text.Json.Serialization;
using Raijin.CombinatoricsService.Application.Features.Problems.BooleanSatisfiability;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Application.Features.Problems;

[JsonPolymorphic]
[JsonDerivedType(typeof(BooleanSatisfiabilityInstanceDto), ProblemTypes.BooleanSatisfiabilityProblem)]
public abstract record InstanceDto;