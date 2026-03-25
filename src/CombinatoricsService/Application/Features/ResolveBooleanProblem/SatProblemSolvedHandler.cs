using FluentResults;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Application.Features.ResolveBooleanProblem;

public sealed class SatProblemSolvedHandler(
    IMediator mediator,
    ILogger<SatProblemSolvedHandler> logger
) : IMessageHandler<ISatProblemSolved>
{
    public async Task Handle(ISatProblemSolved message, CancellationToken cancellationToken)
    {
        Result result = await mediator.Send(new ResolveBooleanProblemCommand(
            message.SatProblemId,
            new SatSolutionDto(message.Solution)
        ), cancellationToken);

        switch (result.IsFailed)
        {
            case true when result.Errors.All(error => error is NotFoundError):
                logger.LogWarning(result.Errors.First().Message);
                logger.LogWarning("The solved sat problem with id {SatProblemId} is not related to a boolean problem",
                    message.SatProblemId);
                return;
            case true:
                throw new MessageProcessingException(result.Errors.First().Message);
        }
    }
}