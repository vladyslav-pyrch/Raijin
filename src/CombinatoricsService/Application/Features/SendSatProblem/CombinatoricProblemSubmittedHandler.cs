using FluentResults;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Application.Features.SendSatProblem;

public sealed class CombinatoricProblemSubmittedHandler(
    IMediator mediator,
    ILogger<CombinatoricProblemSubmittedHandler> logger
) : IMessageHandler<ICombinatoricProblemSubmitted>
{
    public async Task Handle(ICombinatoricProblemSubmitted message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling ICombinatoricProblemSubmitted event for problem {CombinatoricProblemId}",
            message.CombinatoricProblemId);

        Result<SendSatProblemResult> result = await mediator.Send(new SendSatProblemCommand(
            Guid.Parse(message.CombinatoricProblemId),
            new MessageContext(message)
        ), cancellationToken);

        if (result.IsFailed)
        {
            logger.LogError("Processing ICombinatoricProblemSubmitted failed for problem {CombinatoricProblemId}: {Error}",
                message.CombinatoricProblemId, result.Errors[0].Message);
            throw new MessageProcessingException(result.Errors[0].Message);
        }

        logger.LogInformation("Finished handling ICombinatoricProblemSubmitted for problem {CombinatoricProblemId}, sent SAT problem {SatProblemId}",
            message.CombinatoricProblemId, result.Value.SatProblemId);
    }
}

