using Raijin.ProblemSolvingService.Application.Features.SolveBooleanExpression;

namespace Raijin.ProblemSolvingService.Api.Endpoints.V1.SolveBooleanExpression;

public sealed record SolveBooleanExpressionRequest(string Expression)
{
    public SolveBooleanExpressionCommand ToSolveBooleanExpressionCommand() => new(Expression);
}