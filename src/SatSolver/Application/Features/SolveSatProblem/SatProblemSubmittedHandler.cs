using FluentResults;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;
using Raijin.SatSolver.Application.Messaging;

namespace Raijin.SatSolver.Application.Features.SolveSatProblem;

public sealed class SatProblemSubmittedHandler(
    IMediator mediator,
    ILogger<SatProblemSubmittedHandler> logger
) : IMessageHandler<ISatProblemSubmitted>
{
    public async Task Handle(ISatProblemSubmitted message, CancellationToken cancellationToken)
    {
        var command = new SolveSatProblemCommand(message.SatProblemId);

        Result result = await mediator.Send(command, cancellationToken);

        if (result.IsFailed)
            throw new MessageProcessingException(result.Errors[0].Message);
    }
}