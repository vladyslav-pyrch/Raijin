using Raijin.SatSolver.Application.Abstractions;
using Raijin.SatSolver.Domain.SatProblems;

namespace Raijin.SatSolver.Worker;

public class Worker(IServiceProvider provider, ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using IServiceScope scope = provider.CreateScope();
            var satSolver = scope.ServiceProvider.GetRequiredService<ISatSolver>();
            var satProblem = SatProblem.Create("p cnf 3 2\n1 -3 0\n-1 2 3 0");

            logger.LogInformation("Solving problem {id} with dimacs\n{dimacs}\n at time {time}",
                satProblem.Id, satProblem.Dimacs, DateTimeOffset.Now);

            int[] solution = await satSolver.SolveAsync(satProblem, stoppingToken);

            logger.LogInformation("Solved problem {id} with solution {solution} at time {time}",
                satProblem.Id, string.Join(", ", solution), DateTimeOffset.Now);

            await Task.Delay(1000, stoppingToken);
        }
    }
}
