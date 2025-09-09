using System.Diagnostics.CodeAnalysis;
using Raijin.ProblemSolvingService.Application.Cqrs;

namespace Raijin.ProblemSolvingService.Application.Features.CommonSat.Commands.SolveSatExpression;

public sealed class SolveSatExpressionCommandHandler : IRequestHandler<SolveSatExpressionCommand, SolveSatExpressionCommandResult>
{
    public Task<SolveSatExpressionCommandResult> Handle(SolveSatExpressionCommand request,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
