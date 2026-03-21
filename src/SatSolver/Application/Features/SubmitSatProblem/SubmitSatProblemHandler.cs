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
    ICorrelationContextAccessor correlationContextAccessor,
    ILogger<SubmitSatProblemHandler> logger
) : IRequestHandler<SubmitSatProblemCommand, SubmitSatProblemResult>
{
    public async Task<Result<SubmitSatProblemResult>> Handle(SubmitSatProblemCommand request,
        CancellationToken cancellationToken)
    {
        Guid satProblemId = request.SatProblemId ?? Guid.CreateVersion7();
        logger.LogInformation("Submitting SAT problem {SatProblemId}", satProblemId);

        var satProblem = SatProblem.Create(satProblemId, request.Dimacs);

        await satProblemRepository.Add(satProblem, cancellationToken);
        await unitOfWork.SaveChanges(cancellationToken);

        await messageBus.Publish<ISatProblemSubmitted>(new
        {
            SatProblemId = satProblemId.ToString(),
            CombinatoricProblemId = (string?)null,
            request.Dimacs,
            correlationContextAccessor.CorrelationContext.CorrelationId
        }, cancellationToken);

        logger.LogInformation("SAT problem {SatProblemId} submitted successfully", satProblemId);
        return new SubmitSatProblemResult(satProblemId);
    }
}