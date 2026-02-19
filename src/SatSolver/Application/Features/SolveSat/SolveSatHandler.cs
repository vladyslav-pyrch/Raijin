using Microsoft.Extensions.Logging;
using Raijin.SatSolver.Application.Abstractions;
using Raijin.SatSolver.Application.Events;
using Raijin.SatSolver.Domain.SatProblems;

namespace Raijin.SatSolver.Application.Features.SolveSat;

public class SolveSatHandler(ISatProblemRepository repository, ISatSolver solver, IEventBus eventBus, ILogger<SolveSatHandler> logger)
{
    public async Task Handle(SolveSatCommand command, CancellationToken cancellationToken)
    {
        var satProblem = SatProblem.Create(command.Dimacs);
        await repository.AddAndSave(satProblem, cancellationToken);

        int[] solution = await solver.Solve(satProblem, cancellationToken);
        satProblem.SetSolution(solution);

        await repository.UpdateAndSave(satProblem, cancellationToken);
        await eventBus.Publish(new SatProblemSolved(Guid.NewGuid(), solution), cancellationToken);
    }
}