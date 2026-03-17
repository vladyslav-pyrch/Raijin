using FluentResults;
using Raijin.Application.Contracts;
using Raijin.SatSolver.Application.Messaging;

namespace Raijin.SatSolver.Application.Features.SolveSatProblem;

public sealed class SatProblemSubmittedHandler(IMediator mediator) : IMessageHandler<ISatProblemSubmitted>
{
    public async Task Handle(ISatProblemSubmitted message, CancellationToken cancellationToken)
    {
        var command = new SolveSatProblemCommand(
            Guid.Parse(message.SatProblemId),
            message.Dimacs,
            new MessageContext(message)
        );

        Result result = await mediator.Send(command, cancellationToken);

        if (result.IsFailed)
            throw new MessageProcessingException(result.Errors[0].Message);
    }
}