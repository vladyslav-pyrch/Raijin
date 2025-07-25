using Raijin.ProblemSolvingService.Application.Cqrs;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Application.Features.CommonSat.Commands.SolveSatProblem;

public class SolveSatProblemCommandHandler : ICommandHandler<SolveSatProblemCommand, SatResult>
{
    public Task<SatResult> Handle(SolveSatProblemCommand command, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("This command handler is not implemented yet.");
    }
}