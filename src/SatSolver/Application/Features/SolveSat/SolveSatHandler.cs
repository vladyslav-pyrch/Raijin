using Microsoft.Extensions.Logging;
using Raijin.SatSolver.Application.Abstractions;
using Raijin.SatSolver.Application.Events;
using Raijin.SatSolver.Domain.SatProblems;

namespace Raijin.SatSolver.Application.Features.SolveSat;

public class SolveSatHandler(ISatProblemRepository repository, ISatSolver solver, IEventBus eventBus, ILogger<SolveSatHandler> logger)
{
    public async Task Handle(SolveSatCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Received command to solve SAT problem with DIMACS: {Dimacs}", command.Dimacs);

        var satProblem = SatProblem.Create(command.Dimacs);
        logger.LogInformation("Created SAT problem with ID: {Id}", satProblem.Id);

        logger.LogInformation("Saving SAT problem with ID: {Id}", satProblem.Id);
        await repository.AddAndSaveAsync(satProblem, cancellationToken);

        logger.LogInformation("Started solving SAT problem with ID: {Id} at time {time}", satProblem.Id, DateTime.Now);
        int[] solution = await solver.SolveAsync(satProblem, cancellationToken);
        logger.LogInformation("Solved SAT problem with ID: {Id} at time: {time}", satProblem.Id, DateTime.Now);
        logger.LogInformation("Solution for SAT problem with ID: {Id} is: {Solution}", satProblem.Id, string.Join(", ", solution));

        logger.LogInformation("Updating SAT problem with ID: {Id} with solution.", satProblem.Id);
        satProblem.SetSolution(solution);
        logger.LogInformation("Satisfiability of SAT problem with ID: {Id} is: {Satisfiability}", satProblem.Id, satProblem.Satisfiability);

        logger.LogInformation("Saving updated SAT problem with ID: {Id} to repository.", satProblem.Id);
        await repository.UpdateAsync(satProblem, cancellationToken);

        logger.LogInformation("Publishing event that SAT problem with ID: {Id} is solved.", satProblem.Id);
        await eventBus.Publish(new SatProblemSolved(Guid.NewGuid(), solution), cancellationToken);
    }
}