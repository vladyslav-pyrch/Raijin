using Raijin.ProblemSolvingService.Application.Features.SolveSatExpression;

namespace Raijin.ProblemSolvingService.Api.Endpoints.V1.SolveSatExpression;

public sealed record SolveSatExpressionRequest(string SatExpression)
{
    public SolveSatExpressionCommand ToSolveSatExpressionCommand() => new(SatExpression);
}