using Raijin.Application.Contracts;
using Raijin.SatSolver.Application.Cqrs;
using Raijin.SatSolver.Application.Messaging;

namespace Raijin.SatSolver.Application.Features.SolveSatProblem;

public class SatProblemSubmittedHandler(IMediator mediator) : IEventHandler<ISatProblemSubmitted>
{
    public Task Handle(ISatProblemSubmitted @event, CancellationToken cancellationToken)
    {
        var command = new SolveSatProblemCommand(@event.SatProblemId, @event.Dimacs);

        return mediator.Send(command, cancellationToken);
    }
}