using FluentResults;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;
using Raijin.SatSolver.Application.Messaging;

namespace Raijin.SatSolver.Application.Features.SubmitSatProblem;

public sealed class SatProblemSentHandler(
    IMediator mediator,
    ILogger<SatProblemSentHandler> logger
) : IMessageHandler<ISatProblemSent>
{
    public async Task Handle(ISatProblemSent message, CancellationToken cancellationToken)
    {
        var command = new SubmitSatProblemCommand(
            message.Dimacs,
            message.SatProblemId
        );

        Result<SubmitSatProblemResult> result = await mediator.Send(command, cancellationToken);

        if (result.IsFailed)
            throw new MessageProcessingException(result.Errors[0].Message);
    }
}