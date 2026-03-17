using FluentResults;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;
using Raijin.SatSolver.Application.Messaging;
using Raijin.SatSolver.Application.Persistence;
using Raijin.SatSolver.Application.Solver;
using Raijin.SatSolver.Domain.SatProblems;

namespace Raijin.SatSolver.Application.Features.SolveSatProblem;

public sealed class SolveSatProblemHandler(
    ISatProblemRepository satProblemRepository,
    IUnitOfWork unitOfWork,
    ISatSolver solver,
    IMessageBus messageBus,
    IMessageContextAccessor messageContextAccessor,
    IMessageIdGenerator messageIdGenerator,
    ILogger<SolveSatProblemHandler> logger
) : IRequestHandler<SolveSatProblemCommand>
{
    public async Task<Result> Handle(SolveSatProblemCommand request, CancellationToken cancellationToken)
    {
        var satProblem = SatProblem.Create(request.SatProblemId, request.Dimacs);
        
        await satProblemRepository.Add(satProblem, cancellationToken);
        await unitOfWork.SaveChanges(cancellationToken);

        int[] solution = await solver.Solve(satProblem, cancellationToken);
        satProblem.SetSolution(solution);

        await satProblemRepository.Update(satProblem, cancellationToken);
        
        await messageBus.Publish<ISatProblemSolved>(new
        {
            MessageId = messageIdGenerator.NextMessageId(),
            CorrelationId = messageContextAccessor.CurrentContext.CorrelationId,
            CausationId = messageContextAccessor.CurrentContext.CausationId,
            SatProblemId = satProblem.Id,
            Solution = solution
        }, cancellationToken);
        
        await unitOfWork.SaveChanges(cancellationToken);

        return Result.Ok();
    }
}