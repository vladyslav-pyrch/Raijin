using Raijin.SatSolver.Application.Abstractions;
using Raijin.SatSolver.Application.Events;
using Raijin.SatSolver.Domain.SatProblems;

namespace Raijin.SatSolver.Application.Features.SolveSat;

public class SolveSatHandler(ISatProblemRepository repository, ISatSolver solver, IEventBus eventBus)
{
    public async Task Handle(SolveSatCommand command, CancellationToken cancellationToken)
    {
        var satProblem = SatProblem.Create(command.Dimacs);

        await repository.AddAndSaveAsync(satProblem, cancellationToken);
        int[] solution = await solver.SolveAsync(satProblem, cancellationToken);

        satProblem.SetSolution(solution);

        await repository.UpdateAsync(satProblem, cancellationToken);
        await eventBus.Publish(new SatProblemSolved(Guid.NewGuid(), solution), cancellationToken);
    }
}