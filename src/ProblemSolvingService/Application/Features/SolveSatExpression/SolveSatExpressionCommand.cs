using FluentResults;
using Raijin.ProblemSolvingService.Application.Cqrs;

namespace Raijin.ProblemSolvingService.Application.Features.SolveSatExpression;

public sealed record SolveSatExpressionCommand(string SatExpression) : IRequest<Result<SolveSatExpressionResult>>;