using FluentResults;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.BooleanProblems;
using Raijin.CombinatoricsService.Domain.CombinatoricProblems;

namespace Raijin.CombinatoricsService.Application.Features.SubmitBooleanProblem;

public sealed class CombinatoricProblemSubmittedHandler(
    IEventStore eventStore,
    IMediator mediator,
    ILogger<CombinatoricProblemSubmittedHandler> logger
) : IMessageHandler<ICombinatoricProblemSubmitted>
{
    public async Task Handle(ICombinatoricProblemSubmitted message, CancellationToken cancellationToken)
    {
        var combinatoricProblem =
            await eventStore.GetById<CombinatoricProblem>(message.CombinatoricProblemId, cancellationToken);

        if (combinatoricProblem is null)
        {
            logger.LogError("Combinatoric problem with id {CombinatoricProblemId} not found",
                message.CombinatoricProblemId);
            throw new MessageProcessingException(
                $"Combinatoric problem with id {message.CombinatoricProblemId} not found");
        }

        BooleanProblem booleanProblem = combinatoricProblem.ReduceToBooleanProblem();

        Result<SubmitBooleanProblemResult> result = await mediator.Send(new SubmitBooleanProblemCommand(
            booleanProblem.Formula,
            booleanProblem.Id
        ), cancellationToken);

        if (result.IsFailed)
            throw new MessageProcessingException(result.Errors.First().Message);
    }
}