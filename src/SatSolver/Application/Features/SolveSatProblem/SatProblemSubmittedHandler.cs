using Raijin.Application.Contracts;
using Raijin.SatSolver.Application.Cqrs;

namespace Raijin.SatSolver.Application.Features.SolveSatProblem;

public class SatProblemSubmittedHandler(IMediator mediator)
{
    public Task Handle(SatProblemSubmitted @event, CancellationToken cancellationToken)
    {
        var command = new SolveSatProblemCommand(@event.SatProblemId, @event.Dimacs);

        return mediator.Send(command, cancellationToken);
    }
}