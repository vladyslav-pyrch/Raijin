using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Raijin.CombinatoricsService.Application.Features.Problems;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;
using Raijin.CombinatoricsService.Infrastructure.Converters;
using Raijin.CombinatoricsService.Infrastructure.Persistence.Models;

namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Repositories;

public class ProblemRepository(CombinatoricsServiceDbContext dbContext, BoolExprJsonConverter boolExprJsonConverter) : IProblemRepository
{
    private JsonSerializerOptions JsonSerializerOptions => new(JsonSerializerDefaults.General)
    {
        Converters = { boolExprJsonConverter }
    };
    
    public async Task<Problem?> GetById(Guid id, CancellationToken cancellationToken)
    {
        ProblemModel? model = await dbContext.Problems
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return model is null ? null : ToDomain(model);
    }

    public Task<GetProblemResult?> GetSummaryById(Guid id, CancellationToken cancellationToken) => dbContext.Problems
        .AsNoTracking()
        .Where(p => p.Id == id)
        .Select(p => new GetProblemResult(
            p.Id,
            p.Name,
            p.Description,
            p.Solver,
            p.ProblemType,
            Enum.Parse<SolvingStatus>(p.SolvingStatus),
            Enum.Parse<Satisfiability>(p.Satisfiability),
            p.CreatedAt,
            p.UpdatedAt,
            p.CompletedAt))
        .FirstOrDefaultAsync(cancellationToken);

    public Task<GetSatEncodingResult?> GetSatEncodingByProblemId(Guid id, CancellationToken cancellationToken) => dbContext.Problems
        .AsNoTracking()
        .Where(p => p.Id == id)
        .Where(p => p.SatEncoding != null)
        .Select(p => p.SatEncoding!)
        .Select(se => new GetSatEncodingResult(
            se.Clauses.SelectMany(c => c.Literals).DefaultIfEmpty().Max(), // Number of variables is the max variable index in the clauses
            se.Clauses.Count,
            se.Clauses.Select(c => (IReadOnlyList<int>)c.Literals.ToList()).ToList()))
        .FirstOrDefaultAsync(cancellationToken);

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
        existingModel.ProblemType = problem.Instance.ProblemType();
        existingModel.Instance = JsonSerializer.SerializeToDocument(problem.Instance, JsonSerializerOptions);
        existingModel.Solution = problem.Solution is null ? null : JsonSerializer.SerializeToDocument(problem.Solution);
        existingModel.SolvingStatus = problem.SolvingStatus.ToString();
        existingModel.Satisfiability = problem.Satisfiability.ToString();
        existingModel.Assignment = problem.Assignment.ToArray();
        existingModel.UpdatedAt = problem.UpdatedAt;
        existingModel.CompletedAt = problem.CompletedAt;

        if (problem.SatEncoding is null)
            existingModel.SatEncoding = null;
        else if (existingModel.SatEncoding is null)
            existingModel.SatEncoding = ToSatEncodingModel(problem.SatEncoding, problem.Id);
        else
            existingModel.SatEncoding.Clauses = problem.SatEncoding.Clauses
                .Select(clause => new ClauseModel { Literals = clause.ToArray() })
                .ToList();
    }

    public async Task<ListProblemsResult> ListProblems(int page, int pageSize, CancellationToken cancellationToken)
    {
        int totalCount = await dbContext.Problems.CountAsync(cancellationToken);
        int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        List<ProblemSummary> items = await dbContext.Problems
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProblemSummary(
                p.Id,
                p.Name,
                p.ProblemType,
                Enum.Parse<SolvingStatus>(p.SolvingStatus),
                Enum.Parse<Satisfiability>(p.Satisfiability),
                p.CreatedAt))
            .ToListAsync(cancellationToken);

        return new ListProblemsResult(items, page, pageSize, totalPages, totalCount);
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
    

    private ProblemModel ToModel(Problem problem) => new()
    {
        Id = problem.Id,
        Name = problem.Name,
        Description = problem.Description,
        ProblemType = problem.Instance.ProblemType(),
        Instance = JsonSerializer.SerializeToDocument(problem.Instance, JsonSerializerOptions),
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

    private Problem ToDomain(ProblemModel model) => Problem.Rehydrate(
        model.Id,
        model.Name,
        model.Description,
        model.CreatedAt,
        model.UpdatedAt,
        model.Solver,
        model.Instance.Deserialize<Instance>(JsonSerializerOptions) ?? throw new InvalidOperationException($"Failed to deserialize instance for problem {model.Id}."),
        model.SatEncoding is null ? null : SatEncoding.Rehydrate(model.SatEncoding.Clauses.Select(IEnumerable<int> (c) => c.Literals)),
        Enum.Parse<SolvingStatus>(model.SolvingStatus),
        Enum.Parse<Satisfiability>(model.Satisfiability),
        model.Assignment,
        model.CompletedAt,
        model.Solution?.Deserialize<Solution>()
    );
}