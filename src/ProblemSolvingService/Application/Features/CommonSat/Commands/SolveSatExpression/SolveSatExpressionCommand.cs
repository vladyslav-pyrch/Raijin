using Raijin.ProblemSolvingService.Application.Cqrs;

namespace Raijin.ProblemSolvingService.Application.Features.CommonSat.Commands.SolveSatExpression;

public sealed record SolveSatExpressionCommand(string Expression) : IRequest<SolveSatExpressionCommandResult>;