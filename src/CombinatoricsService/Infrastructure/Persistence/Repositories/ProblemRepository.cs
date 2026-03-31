using System.Text.Json;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;
using Raijin.CombinatoricsService.Infrastructure.Persistence.Models;

namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Repositories;

public class ProblemRepository(CombinatoricsServiceDbContext dbContext) : IProblemRepository
{
    public async Task<Problem?> GetById(Guid id, CancellationToken cancellationToken)
    {
        ProblemModel? model = await dbContext.Problems.FindAsync([id], cancellationToken);

        if (model is null)
            return null;

        return Problem.Rehydrate(
            model.Id,
            model.Name,
            model.Description,
            model.ProblemType,
            model.Instance is { } instance ? instance.Deserialize<Instance>() : null,
            model.SatEncoding is { } encoding
                ? SatEncoding.Rehydrate(
                    encoding.Dimacs,
                    encoding.VariableMap.Deserialize<VariableMap>()!
                )
                : null,
            model.SatRun is { } run
                ? SatRun.Rehydrate(
                    Enum.Parse<SatRunStatus>(run.Status),
                    Enum.Parse<Satisfiability>(run.Satisfiability),
                    run.Assignment,
                    run.CreatedAt,
                    run.CompletedAt
                )
                : null,
            model.Solution is { } solution ? solution.Deserialize<Solution>() : null
        );

        return null;
    }

    public Task Add(Problem problem, CancellationToken cancellationToken)
    {
        var model = new ProblemModel
        {
            Id = problem.Id,
            Name = problem.Name,
            Description = problem.Description,
            ProblemType = problem.ProblemType,
            Instance = JsonSerializer.SerializeToDocument(problem.Instance),
            SatEncoding = problem.SatEncoding is { } encoding
                ? new SatEncodingModel
                {
                    Dimacs = encoding.Dimacs,
                    VariableMap = JsonSerializer.SerializeToDocument(encoding.VariableMap)
                }
                : null,
            SatRun = problem.SatRun is { } run
                ? new SatRunModel
                {
                    Satisfiability = run.Satisfiability.ToString(),
                    Status = run.Status.ToString(),
                    CreatedAt = run.CreatedAt,
                    CompletedAt = run.CompletedAt,
                    Assignment = problem.SatRun.Assignment.ToArray()
                }
                : null,
            Solution = JsonSerializer.SerializeToDocument(problem.Solution)
        };

        dbContext.Problems.Add(model);

        return Task.CompletedTask;
    }

    public async Task Update(Problem problem, CancellationToken cancellationToken)
    {
        ProblemModel? model = await dbContext.Problems.FindAsync([problem.Id], cancellationToken);

        if (model is null)
            throw new InvalidOperationException("Cannot update an entity that does not exists");

        model.Name = problem.Name;
        model.Description = problem.Description;
        model.ProblemType = problem.ProblemType;
        model.Instance = JsonSerializer.SerializeToDocument(problem.Instance);
        model.SatEncoding = problem.SatEncoding is { } encoding
            ? new SatEncodingModel
            {
                Dimacs = encoding.Dimacs,
                VariableMap = JsonSerializer.SerializeToDocument(encoding.VariableMap)
            }
            : null;
        model.SatRun = problem.SatRun is { } run
            ? new SatRunModel
            {
                Satisfiability = run.Satisfiability.ToString(),
                Status = run.Status.ToString(),
                CreatedAt = run.CreatedAt,
                CompletedAt = run.CompletedAt,
                Assignment = problem.SatRun.Assignment.ToArray()
            }
            : null;
        model.Solution = JsonSerializer.SerializeToDocument(problem.Solution);
    }
}