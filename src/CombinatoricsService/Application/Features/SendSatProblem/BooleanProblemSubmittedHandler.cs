using FluentResults;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Logic;

namespace Raijin.CombinatoricsService.Application.Features.SendSatProblem;

public sealed class BooleanProblemSubmittedHandler(
    IBooleanProblemRepository booleanProblemRepository,
    IMediator mediator,
    ILogger<BooleanProblemSubmittedHandler> logger
) : IMessageHandler<IBooleanProblemSubmitted>
{
    public async Task Handle(IBooleanProblemSubmitted message, CancellationToken cancellationToken)
    {
        BooleanProblem? booleanProblem =
            await booleanProblemRepository.GetById(Guid.Parse(message.BooleanProblemId), cancellationToken);

        if (booleanProblem is null)
        {
            logger.LogError("Boolean problem with id {BooleanProblemId} not found",
                message.BooleanProblemId);
            throw new MessageProcessingException(
                $"Combinatoric problem with id {message.BooleanProblemId} not found");
        }

        SatReduction satReduction = booleanProblem.ReduceToSat();

        Result<SendSatProblemResult> result = await mediator.Send(new SendSatProblemCommand(
            satReduction.Id,
            satReduction.Dimacs
        ), cancellationToken);

        if (result.IsFailed)
            throw new MessageProcessingException(result.Errors[0].Message);
    }
}