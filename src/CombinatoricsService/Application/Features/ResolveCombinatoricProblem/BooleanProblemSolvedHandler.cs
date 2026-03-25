using FluentResults;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Application.Features.ResolveCombinatoricProblem;

public sealed class BooleanProblemSolvedHandler(
    IMediator mediator,
    ILogger<BooleanProblemSolvedHandler> logger
) : IMessageHandler<IBooleanProblemSolved>
{
    public async Task Handle(IBooleanProblemSolved message, CancellationToken cancellationToken)
    {
        Result result = await mediator.Send(new ResolveCombinatoricProblemCommand(
            message.BooleanProblemId,
            new BooleanProblemSolutionDto(message.Solution)
        ), cancellationToken);

        switch (result.IsFailed)
        {
            case true when result.Errors.All(error => error is NotFoundError):
                logger.LogWarning(result.Errors.First().Message);
                logger.LogWarning(
                    "The solved boolean problem with id {BooleanProblemId} is not related to a combinatoric problem",
                    message.BooleanProblemId);
                return;
            case true:
                throw new MessageProcessingException(result.Errors.First().Message);
        }
    }
}