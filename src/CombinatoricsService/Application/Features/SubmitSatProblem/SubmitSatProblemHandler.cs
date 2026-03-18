using FluentResults;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.CombinatoricProblems;
using Raijin.CombinatoricsService.Domain.Logic;

namespace Raijin.CombinatoricsService.Application.Features.SubmitSatProblem;

public sealed class SubmitSatProblemHandler(
    ICombinatoricProblemRepository combinatoricProblemRepository,
    IMessageBus messageBus,
    IMessageIdGenerator messageIdGenerator,
    IMessageContextAccessor messageContextAccessor,
    IUnitOfWork unitOfWork,
    ILogger<SubmitSatProblemHandler> logger
) : IRequestHandler<SubmitSatProblemCommand, SubmitSatProblemResult>
{
    public async Task<Result<SubmitSatProblemResult>> Handle(
        SubmitSatProblemCommand request,
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

        TseitinTransformResult tseitinResult = combinatoricProblem.ToFormula().TseitinTransform();
        string dimacs = tseitinResult.Problem.ToDimacs();
        var satProblemId = Guid.CreateVersion7();

        logger.LogInformation("Combinatoric problem {CombinatoricProblemId} transformed to SAT problem {SatProblemId}",
            request.CombinatoricProblemId, satProblemId);

        await messageBus.Publish<ISatProblemSubmitted>(new
        {
            MessageId = messageIdGenerator.NextMessageId(),
            CorrelationId = messageContextAccessor.CurrentContext.CorrelationId,
            CausationId = messageContextAccessor.CurrentContext.CausationId,
            Timestamp = DateTimeOffset.UtcNow,
            SatProblemId = satProblemId.ToString(),
            CombinatoricProblemId = request.CombinatoricProblemId.ToString(),
            Dimacs = dimacs,
        }, cancellationToken);

        await unitOfWork.SaveChanges(cancellationToken);

        logger.LogInformation("SAT problem {SatProblemId} submitted for combinatoric problem {CombinatoricProblemId}",
            satProblemId, request.CombinatoricProblemId);
        return new SubmitSatProblemResult(satProblemId);
    }
}


