using FluentResults;
using Raijin.Application.Contracts;
using Raijin.SatSolver.Application.Cqrs;
using Raijin.SatSolver.Application.Messaging;

namespace Raijin.SatSolver.Application.Features.SolveSatProblem;

public class SatProblemSubmittedHandler(IMediator mediator) : IMessageHandler<ISatProblemSubmitted>
{
    public async Task Handle(ISatProblemSubmitted message, CancellationToken cancellationToken)
    {
        var command = new SolveSatProblemCommand(message.SatProblemId, message.Dimacs);

        Result result = await mediator.Send(command, cancellationToken);

        if (result.IsFailed)
            throw new MessageProcessingException(result.Errors[0].Message);
    }
}