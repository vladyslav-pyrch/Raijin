using FluentResults;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Application.Features.ResolveCombinatoricProblem;

public sealed class SatProblemSolvedHandler(
    IMediator mediator,
    ILogger<SatProblemSolvedHandler> logger
) : IMessageHandler<ISatProblemSolved>
{
    public async Task Handle(ISatProblemSolved message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling ISatProblemSolved event for SAT problem {SatProblemId}",
            message.SatProblemId);

        Result result = await mediator.Send(new ResolveCombinatoricProblemCommand(
            Guid.Parse(message.SatProblemId),
            message.Solution,
            new MessageContext(message)
        ), cancellationToken);

        if (result.IsFailed)
        {
            if (result.Errors.All(e => e is NotFoundError))
            {
                logger.LogWarning("No combinatoric problem found for SAT problem {SatProblemId}", message.SatProblemId);
                return;
            }

            logger.LogError("Processing ISatProblemSolved failed for SAT problem {SatProblemId}: {Errors}",
                message.SatProblemId, string.Join("; ", result.Errors.Select(e => e.Message)));
            throw new MessageProcessingException(result.Errors[0].Message);
        }

        logger.LogInformation(
            "Finished handling ISatProblemSolved for SAT problem {SatProblemId}",
            message.SatProblemId);
    }
}
