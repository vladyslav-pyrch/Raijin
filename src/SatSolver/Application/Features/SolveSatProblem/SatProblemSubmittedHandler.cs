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
        logger.LogInformation("Handling ISatProblemSubmitted event for SAT problem {SatProblemId}", message.SatProblemId);

        var command = new SolveSatProblemCommand(
            Guid.Parse(message.SatProblemId),
            message.Dimacs,
            new MessageContext(message)
        );

        Result result = await mediator.Send(command, cancellationToken);

        if (result.IsFailed)
        {
            logger.LogError("Processing ISatProblemSubmitted failed for SAT problem {SatProblemId}: {Error}",
                message.SatProblemId, result.Errors[0].Message);
            throw new MessageProcessingException(result.Errors[0].Message);
        }

        logger.LogInformation("Finished handling ISatProblemSubmitted event for SAT problem {SatProblemId}", message.SatProblemId);
    }
}