using Raijin.ProblemSolvingService.Application.Cqrs;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Application.Features.CommonSat.Commands.SolveSatProblem;

public class SolveSatProblemCommandHandler(ISatSolver satSolver) : ICommandHandler<SolveSatProblemCommand, SatResult>
{
    public Task<SatResult> Handle(SolveSatProblemCommand command, CancellationToken cancellationToken = default)
        => satSolver.Solve(command.SatProblem, cancellationToken);
}