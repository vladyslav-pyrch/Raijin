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
        logger.LogInformation("Starting to solve SAT problem {SatProblemId}", request.SatProblemId);

        var satProblem = SatProblem.Create(request.SatProblemId, request.Dimacs);

        await using (ITransaction transaction = await unitOfWork.BeginTransaction(cancellationToken))
        {
            await satProblemRepository.Add(satProblem, cancellationToken);
            await transaction.SaveChanges(cancellationToken);
            await transaction.Commit(cancellationToken);
        }

        logger.LogInformation("Invoking SAT solver for problem {SatProblemId}", request.SatProblemId);
        int[] solution = await solver.Solve(satProblem, cancellationToken);
        satProblem.SetSolution(solution);

        logger.LogInformation("SAT problem {SatProblemId} solved with satisfiability {Satisfiability}, solution length {SolutionLength}",
            request.SatProblemId, satProblem.Satisfiability, solution.Length);

        await satProblemRepository.Update(satProblem, cancellationToken);

        await messageBus.Publish<ISatProblemSolved>(new
        {
            MessageId = messageIdGenerator.NextMessageId(),
            CorrelationId = messageContextAccessor.CurrentContext.CorrelationId,
            CausationId = messageContextAccessor.CurrentContext.CausationId,
            Timestamp = DateTime.UtcNow,
            SatProblemId = satProblem.Id.ToString(),
            Solution = solution
        }, cancellationToken);

        await unitOfWork.SaveChanges(cancellationToken);

        logger.LogInformation("SAT problem {SatProblemId} solve result published successfully", request.SatProblemId);
        return Result.Ok();
    }
}