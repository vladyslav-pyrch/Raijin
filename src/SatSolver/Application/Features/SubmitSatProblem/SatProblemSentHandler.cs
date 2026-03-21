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
        logger.LogInformation("Handling ISatProblemSent event for SAT problem {SatProblemId}", message.SatProblemId);

        var command = new SubmitSatProblemCommand(
            message.Dimacs,
            new MessageContext(message),
            Guid.Parse(message.SatProblemId)
        );

        Result<SubmitSatProblemResult> result = await mediator.Send(command, cancellationToken);

        if (result.IsFailed)
        {
            logger.LogError("Processing ISatProblemSent failed for SAT problem {SatProblemId}: {Error}",
                message.SatProblemId, result.Errors[0].Message);
            throw new MessageProcessingException(result.Errors[0].Message);
        }

        logger.LogInformation("Finished handling ISatProblemSent event for SAT problem {SatProblemId}, submitted as {SubmittedSatProblemId}",
            message.SatProblemId, result.Value.SatProblemId);
    }
}

