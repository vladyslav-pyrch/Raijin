using Raijin.ProblemSolvingService.Application.Cqrs;

namespace Raijin.ProblemSolvingService.Application.Features.CommonSat.Commands.SolveSatProblem;

public class SolveSatProblemCommandHandler : ICommandHandler<SolveSatProblemCommand, SolveSatProblemCommandResult>
{
    public Task<SolveSatProblemCommandResult> Handle(SolveSatProblemCommand command, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}