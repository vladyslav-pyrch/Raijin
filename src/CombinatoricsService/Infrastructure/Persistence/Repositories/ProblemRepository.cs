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
        var existingModel = await dbContext.Problems
            .FirstOrDefaultAsync(p => p.Id == problem.Id, cancellationToken);

        if (existingModel is null)
            throw new InvalidOperationException($"Problem {problem.Id} not found.");

        existingModel.Name = problem.Name;
        existingModel.Description = problem.Description;
        existingModel.Solver = problem.Solver;
        existingModel.Instance = JsonSerializer.SerializeToDocument(problem.Instance);
        existingModel.Solution = problem.Solution is null ? null : JsonSerializer.SerializeToDocument(problem.Solution);
        existingModel.SolvingStatus = problem.SolvingStatus.ToString();
        existingModel.Satisfiability = problem.Satisfiability.ToString();
        existingModel.Assignment = problem.Assignment.ToArray();
        existingModel.UpdatedAt = problem.UpdatedAt;
        existingModel.CompletedAt = problem.CompletedAt;

        if (problem.SatEncoding is null)
        {
            existingModel.SatEncoding = null;
        }
        else if (existingModel.SatEncoding is null)
        {
            existingModel.SatEncoding = ToSatEncodingModel(problem.SatEncoding, problem.Id);
        }
        else
        {
            existingModel.SatEncoding.Clauses = problem.SatEncoding.Clauses
                .Select(clause => new ClauseModel { Literals = clause.ToArray() })
                .ToList();
        }
    }

    public async Task<(IReadOnlyList<Problem> Items, int TotalCount)> GetPage(int page, int pageSize, CancellationToken cancellationToken)
    {
        int totalCount = await dbContext.Problems.CountAsync(cancellationToken);

        var models = await dbContext.Problems
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (models.Select(ToDomain).ToList(), totalCount);
    }

    public async Task<Problem?> GetOldestPendingWithLock(CancellationToken cancellationToken)
    {
        var model = await dbContext.Problems
            .FromSql(
                $"""
                SELECT * FROM "Problems"
                WHERE "SolvingStatus" = 'Pending'
                ORDER BY "UpdatedAt" ASC
                LIMIT 1
                FOR UPDATE SKIP LOCKED
                """)
            .AsTracking()
            .FirstOrDefaultAsync(cancellationToken);

        return model is null ? null : ToDomain(model);
    }

    private static ProblemModel ToModel(Problem problem) => new()
    {
        Id = problem.Id,
        Name = problem.Name,
        Description = problem.Description,
        Instance = JsonSerializer.SerializeToDocument(problem.Instance),
        Solution = problem.Solution is null ? null : JsonSerializer.SerializeToDocument(problem.Solution),
        Solver = problem.Solver,
        SolvingStatus = problem.SolvingStatus.ToString(),
        Satisfiability = problem.Satisfiability.ToString(),
        Assignment = problem.Assignment.ToArray(),
        CreatedAt = problem.CreatedAt,
        UpdatedAt = problem.UpdatedAt,
        CompletedAt = problem.CompletedAt,
        SatEncoding = problem.SatEncoding is null ? null : ToSatEncodingModel(problem.SatEncoding, problem.Id)
    };

    private static SatEncodingModel ToSatEncodingModel(SatEncoding encoding, Guid problemId) => new()
    {
        ProblemId = problemId,
        Clauses = encoding.Clauses
            .Select(clause => new ClauseModel { Literals = clause.ToArray() })
            .ToList()
    };

    private static Problem ToDomain(ProblemModel model) => Problem.Rehydrate(
        model.Id,
        model.Name,
        model.Description,
        model.CreatedAt,
        model.UpdatedAt,
        model.Solver,
        model.Instance.Deserialize<Instance>() ?? throw new InvalidOperationException($"Failed to deserialize instance for problem {model.Id}."),
        model.SatEncoding is null ? null : SatEncoding.Rehydrate(model.SatEncoding.Clauses.Select(c => (IEnumerable<int>)c.Literals)),
        Enum.Parse<SolvingStatus>(model.SolvingStatus),
        Enum.Parse<Satisfiability>(model.Satisfiability),
        model.Assignment,
        model.CompletedAt,
        model.Solution?.Deserialize<Solution>()
    );
}