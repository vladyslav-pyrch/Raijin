using Raijin.SatSolver.Application.Abstractions;
using Raijin.SatSolver.Application.Events;

namespace Raijin.SatSolver.Application.Features.SolveSat;

public class SatProblemGeneratedHandler(IMediator mediator) : IEventHandler<SatProblemGenerated>
{
    public Task Handle(SatProblemGenerated @event, CancellationToken cancellationToken)
    {
        var command = new SolveSatCommand(@event.Dimacs);

        return mediator.Send(command, cancellationToken);
    }
}