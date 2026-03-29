using System.Text.Json;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;
using Raijin.CombinatoricsService.Infrastructure.Persistence.Models;

namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Repositories;

public class ProblemRepository(CombinatoricsServiceDbContext dbContext) : IProblemRepository
{
    public async Task<Problem?> GetById(Guid id, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task Add(Problem problem, CancellationToken cancellationToken)
    {
        var model = new ProblemModel
        {
            Id = problem.Id,
            Name = problem.Name,
            Description = problem.Description,
            ProblemKind = problem.ProblemKind,
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
}