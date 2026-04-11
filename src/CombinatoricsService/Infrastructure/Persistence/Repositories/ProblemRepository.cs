using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;
using Raijin.CombinatoricsService.Infrastructure.Persistence.Models;

namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Repositories;

public class ProblemRepository(CombinatoricsServiceDbContext dbContext) : IProblemRepository
{
    public async Task<Problem?> GetById(Guid id, CancellationToken cancellationToken)
    {
        var model = await dbContext.Problems
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return model is null ? null : ToDomain(model);
    }

    public Task Add(Problem problem, CancellationToken cancellationToken)
    {
        dbContext.Problems.Add(ToModel(problem));
        return Task.CompletedTask;
    }

    public async Task Update(Problem problem, CancellationToken cancellationToken)
    {
        var existingModel = await dbContext.Problems.FindAsync([problem.Id], cancellationToken);

        if (existingModel is null)
            throw new InvalidOperationException($"Problem {problem.Id} not found.");

        if (existingModel.SatRunId is not null && existingModel.SatRunId != problem.SatRunId)
        {
            var staleRun = await dbContext.SatRuns
                .FindAsync([existingModel.SatRunId.Value], cancellationToken);

            if (staleRun is not null)
                dbContext.SatRuns.Remove(staleRun);
        }

        existingModel.Name = problem.Name;
        existingModel.Description = problem.Description;
        existingModel.SatRunId = problem.SatRunId;
        existingModel.Instance = JsonSerializer.SerializeToDocument(problem.Instance);
        existingModel.Solution = JsonSerializer.SerializeToDocument(problem.Solution);
    }

    private static ProblemModel ToModel(Problem problem) => new()
    {
        Id = problem.Id,
        Name = problem.Name,
        Description = problem.Description,
        SatRunId = problem.SatRunId,
        Instance = JsonSerializer.SerializeToDocument(problem.Instance),
        Solution = JsonSerializer.SerializeToDocument(problem.Solution)
    };

    private static Problem ToDomain(ProblemModel model) => Problem.Rehydrate(
        model.Id,
        model.Name,
        model.Description,
        model.Instance.Deserialize<Instance>(),
        model.SatRunId,
        model.Solution.Deserialize<Solution>()
    );
}