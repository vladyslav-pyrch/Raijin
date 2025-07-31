using FluentValidation;
using Raijin.ProblemSolvingService.Application.Cqrs;
using Raijin.ProblemSolvingService.Application.Features.CommonSat.Commands.SolveSatProblemInternal;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Application.Features.CommonSat.Commands.SolveSatProblem;

public class SolveSatProblemCommandHandler(ICommandDispatcher dispatcher)
    : ICommandHandler<SolveSatProblemCommand, SolveSatProblemCommandResult>
{
    public async Task<SolveSatProblemCommandResult> Handle(SolveSatProblemCommand command, CancellationToken cancellationToken = default)
    {
        await new SolveSatProblemCommandValidator().ValidateAndThrowAsync(command, cancellationToken);

        SolveSatProblemInternalCommand internalCommand = command.ToInternalCommand();

        SatResult satResult = await dispatcher.Dispatch<SolveSatProblemInternalCommand, SatResult>(internalCommand, cancellationToken);

        return SolveSatProblemCommandResult.FromSatResult(satResult);
    }
}