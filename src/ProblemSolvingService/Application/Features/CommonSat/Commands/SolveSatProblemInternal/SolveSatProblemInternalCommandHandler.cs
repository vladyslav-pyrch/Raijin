using Raijin.ProblemSolvingService.Application.Cqrs;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Application.Features.CommonSat.Commands.SolveSatProblemInternal;

public sealed class SolveSatProblemInternalCommandHandler(ISatSolver satSolver) : IRequestHandler<SolveSatProblemInternalCommand, SatResult>
{
    public Task<SatResult> Handle(SolveSatProblemInternalCommand internalCommand, CancellationToken cancellationToken = default)
        => satSolver.Solve(internalCommand.SatProblem, cancellationToken);
}