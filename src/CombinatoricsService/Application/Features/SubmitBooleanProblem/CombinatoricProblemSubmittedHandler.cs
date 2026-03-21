using FluentResults;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.CombinatoricProblems;
using Raijin.CombinatoricsService.Domain.Logic;

namespace Raijin.CombinatoricsService.Application.Features.SubmitBooleanProblem;

public class CombinatoricProblemSubmittedHandler(
    ICombinatoricProblemRepository combinatoricProblemRepository,
    IMediator mediator,
    ILogger<CombinatoricProblemSubmittedHandler> logger
) : IMessageHandler<ICombinatoricProblemSubmitted>
{
    public async Task Handle(ICombinatoricProblemSubmitted message, CancellationToken cancellationToken)
    {
        CombinatoricProblem? combinatoricProblem =
            await combinatoricProblemRepository.GetById(Guid.Parse(message.CombinatoricProblemId), cancellationToken);

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
            new MessageContext(message),
            booleanProblem.Id
        ), cancellationToken);

        if (result.IsFailed)
            throw new MessageProcessingException(result.Errors[0].Message);
    }
}