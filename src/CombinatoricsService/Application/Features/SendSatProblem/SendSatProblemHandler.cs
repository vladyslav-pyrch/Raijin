using FluentResults;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.CombinatoricProblems;
using Raijin.CombinatoricsService.Domain.Logic;

namespace Raijin.CombinatoricsService.Application.Features.SendSatProblem;

public sealed class SendSatProblemHandler(
    ICombinatoricProblemRepository combinatoricProblemRepository,
    IMessageBus messageBus,
    IMessageIdGenerator messageIdGenerator,
    IMessageContextAccessor messageContextAccessor,
    ILogger<SendSatProblemHandler> logger
) : IRequestHandler<SendSatProblemCommand, SendSatProblemResult>
{
    public async Task<Result<SendSatProblemResult>> Handle(
        SendSatProblemCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Transforming combinatoric problem {CombinatoricProblemId} to SAT", request.CombinatoricProblemId);

        CombinatoricProblem? combinatoricProblem = await combinatoricProblemRepository.GetById(
            request.CombinatoricProblemId, cancellationToken
        );

        if (combinatoricProblem is null)
        {
            logger.LogWarning("CombinatoricProblem {CombinatoricProblemId} was not found", request.CombinatoricProblemId);
            return Result.Fail($"CombinatoricProblem '{request.CombinatoricProblemId}' was not found.");
        }
        
        SatReduction satReduction = combinatoricProblem.ToBooleanProblem().GetSatReduction();

        logger.LogInformation("Combinatoric problem {CombinatoricProblemId} transformed to SAT problem",
            request.CombinatoricProblemId);

        await messageBus.Publish<ISatProblemSent>(new
        {
            MessageId = messageIdGenerator.NextMessageId(),
            CorrelationId = messageContextAccessor.CurrentContext.CorrelationId,
            CausationId = messageContextAccessor.CurrentContext.CausationId,
            Timestamp = DateTimeOffset.UtcNow,
            SatProblemId = satReduction.Id.ToString(),
            Dimacs = satReduction.Dimacs,
        }, cancellationToken);

        logger.LogInformation("SAT problem {SatProblemId} sent", satReduction.Id);
        return new SendSatProblemResult(satReduction.Id);
    }
}

