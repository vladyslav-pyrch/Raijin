using FluentResults;
using Raijin.ProblemSolvingService.Application.Cqrs;

namespace Raijin.ProblemSolvingService.Application.Features.SolveBooleanExpression;

public sealed record SolveBooleanExpressionCommand(string Expression) : IRequest<Result<SolveBooleanExpressionResult>>;