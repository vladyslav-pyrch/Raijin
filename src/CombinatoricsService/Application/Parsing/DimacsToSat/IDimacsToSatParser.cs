using FluentResults;
using Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;

namespace Raijin.CombinatoricsService.Application.Parsing.DimacsToSat;

public interface IDimacsToSatParser
{
    public Result<BooleanSatisfiabilityInstance> Parse(string input);
}