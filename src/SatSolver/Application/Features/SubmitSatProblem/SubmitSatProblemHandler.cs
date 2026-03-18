using FluentResults;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;
using Raijin.SatSolver.Application.Messaging;
using Raijin.SatSolver.Application.Persistence;
using Raijin.SatSolver.Domain.SatProblems;

namespace Raijin.SatSolver.Application.Features.SubmitSatProblem;

public sealed class SubmitSatProblemHandler(
    ISatProblemRepository satProblemRepository,
    IUnitOfWork unitOfWork,
    IMessageBus messageBus,
    IMessageContextAccessor messageContextAccessor,
    IMessageIdGenerator messageIdGenerator,
    ILogger<SubmitSatProblemHandler> logger
) : IRequestHandler<SubmitSatProblemCommand, SubmitSatProblemResult>
{
    public async Task<Result<SubmitSatProblemResult>> Handle(SubmitSatProblemCommand request,
        CancellationToken cancellationToken)
    {
        var satProblemId = Guid.CreateVersion7();
        logger.LogInformation("Submitting SAT problem {SatProblemId}", satProblemId);

        var satProblem = SatProblem.Create(satProblemId, request.Dimacs);
        await satProblemRepository.Add(satProblem, cancellationToken);

        await messageBus.Publish<ISatProblemSubmitted>(message: new
        {
            MessageId = messageIdGenerator.NextMessageId(),
            CorrelationId = messageContextAccessor.CurrentContext.CorrelationId,
            CausationId = messageContextAccessor.CurrentContext.CausationId,
            Timestamp = DateTimeOffset.UtcNow,
            SatProblemId = satProblemId.ToString(),
            CombinatoricProblemId = (string?)null,
            Dimacs = request.Dimacs,
        }, cancellationToken);

        await unitOfWork.SaveChanges(cancellationToken);

        logger.LogInformation("SAT problem {SatProblemId} submitted successfully", satProblemId);
        return new SubmitSatProblemResult(satProblemId);
    }
}