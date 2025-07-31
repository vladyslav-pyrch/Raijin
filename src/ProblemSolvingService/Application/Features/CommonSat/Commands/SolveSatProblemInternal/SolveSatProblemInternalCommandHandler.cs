using Raijin.ProblemSolvingService.Application.Cqrs;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Application.Features.CommonSat.Commands.SolveSatProblem;

public class SolveSatProblemInternalCommandHandler(ISatSolver satSolver) : ICommandHandler<SolveSatProblemInternalCommand, SatResult>
{
    public Task<SatResult> Handle(SolveSatProblemInternalCommand internalCommand, CancellationToken cancellationToken = default)
        => satSolver.Solve(internalCommand.SatProblem, cancellationToken);
}