using Microsoft.Extensions.Logging;
using Raijin.SatSolver.Application.Cqrs;
using Raijin.SatSolver.Application.Messaging;
using Raijin.SatSolver.Application.Persistence;
using Raijin.SatSolver.Application.Solver;
using Raijin.SatSolver.Domain.SatProblems;

namespace Raijin.SatSolver.Application.Features.SolveSatProblem;

public class SolveSatProblemHandler(ISatProblemRepository repository, ISatSolver solver, IMessageBus messageBus, ILogger<SolveSatProblemHandler> logger) : IRequestHandler<SolveSatProblemCommand>
{
    public async Task Handle(SolveSatProblemCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Received SolveSatCommand with Dimacs: {Dimacs}", command.Dimacs);

        var satProblem = SatProblem.Create(command.SatProblemId, command.Dimacs);
        await repository.AddAndSave(satProblem, cancellationToken);
        // await messageBus.Publish(new SatProblemCreated(Guid.NewGuid(), solution), cancellationToken);

        int[] solution = await solver.Solve(satProblem, cancellationToken);
        satProblem.SetSolution(solution);

        await repository.UpdateAndSave(satProblem, cancellationToken);
        // await messageBus.Publish(new SatProblemSolved(Guid.NewGuid(), solution), cancellationToken);
    }
}