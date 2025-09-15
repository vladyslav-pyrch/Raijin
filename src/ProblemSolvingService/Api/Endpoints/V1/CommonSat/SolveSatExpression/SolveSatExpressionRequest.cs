using Raijin.ProblemSolvingService.Application.Features.CommonSat.Commands.SolveSatExpression;

namespace Raijin.ProblemSolvingService.Api.Endpoints.V1.CommonSat.SolveSatExpression;

public sealed record SolveSatExpressionRequest(string SatExpression)
{
    public SolveSatExpressionCommand ToSolveSatExpressionCommand() => new(SatExpression);
}