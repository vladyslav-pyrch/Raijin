using FluentResults;
using Raijin.ProblemSolvingService.Application.Cqrs;

namespace Raijin.ProblemSolvingService.Application.Features.CommonSat.Commands.SolveSatExpression;

public sealed record SolveSatExpressionCommand(string SatExpression) : IRequest<Result<SolveSatExpressionCommandResult>>;