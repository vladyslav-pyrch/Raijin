using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Raijin.CombinatoricsService.Application.Features.Problems;
using Raijin.CombinatoricsService.Application.Parsing.DimacsToSat;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;
using Raijin.CombinatoricsService.Infrastructure.Converters;
using Raijin.CombinatoricsService.Infrastructure.Persistence.Models;

namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Repositories;

public class ProblemRepository(
    CombinatoricsServiceDbContext dbContext,
    BoolExprJsonConverter boolExprJsonConverter,
    DimacsToSatParser dimacsToSatParser,
    ILogger<ProblemRepository> logger
) : IProblemRepository
{
    private JsonSerializerOptions JsonSerializerOptions => new(JsonSerializerDefaults.General)
    {
        Converters =
        {
            boolExprJsonConverter
        }
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
            p.StartedSolvingAt,
            p.CompletedAt,
            p.ElapsedTime))
        .FirstOrDefaultAsync(cancellationToken);

    public async Task<GetSatEncodingResult?> GetSatEncodingByProblemId(Guid id, CancellationToken cancellationToken)
    {
        string? dimacsEncoding = await dbContext.Problems
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => p.DimacsEncoding) // Get the clauses of the SAT encoding
            .FirstOrDefaultAsync(cancellationToken);
        
        if (dimacsEncoding is null)
            return null;
        
        IEnumerable<IEnumerable<int>> clauses;

        try
        {
            clauses = dimacsToSatParser.ParseForSatEncoding(dimacsEncoding).ToArray();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to reconstruct SAT encoding. ProblemId={ProblemId}", id);
            throw;
        }

        return new GetSatEncodingResult(
            clauses.SelectMany(c => c).DefaultIfEmpty().Max(), // Number of variables is the max variable index in the clauses
            clauses.Count(),
            clauses.Select(c => c.ToList()).ToList()
        );
    }

    public Task Add(Problem problem, CancellationToken cancellationToken)
    {
        dbContext.Problems.Add(ToModel(problem));
        return Task.CompletedTask;
    }

    public async Task Update(Problem problem, CancellationToken cancellationToken)
    {
        ProblemModel? existingModel = await dbContext.Problems
            .FirstOrDefaultAsync(p => p.Id == problem.Id, cancellationToken);

        if (existingModel is null)
        {
            logger.LogError("Cannot update missing problem. ProblemId={ProblemId}", problem.Id);
            throw new InvalidOperationException($"Problem {problem.Id} not found.");
        }

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
        existingModel.StartedSolvingAt = problem.StartedSolvingAt;
        existingModel.CompletedAt = problem.CompletedAt;
        existingModel.ElapsedTime = problem.ElapsedTime;
        existingModel.DimacsEncoding = problem.SatEncoding?.ToDimacs();
    }

    public async Task Delete(Guid id, CancellationToken cancellationToken)
    {
        ProblemModel? existingModel = await dbContext.Problems
            .FirstOrDefaultAsync(problem => problem.Id == id, cancellationToken);

        if (existingModel is null)
        {
            logger.LogError("Cannot delete missing problem. ProblemId={ProblemId}", id);
            throw new InvalidOperationException($"Problem {id} not found.");
        }

        dbContext.Problems.Remove(existingModel);
    }

    public async Task<ListProblemsResult> ListProblems(int page, int pageSize, CancellationToken cancellationToken)
    {
        int totalCount = await dbContext.Problems.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

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
        ProblemModel? model = await dbContext.Problems
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
        StartedSolvingAt = problem.StartedSolvingAt,
        CompletedAt = problem.CompletedAt,
        ElapsedTime = problem.ElapsedTime,
        DimacsEncoding = problem.SatEncoding?.ToDimacs()
    };

    private Problem ToDomain(ProblemModel model)
    {
        Instance instance;
        SatEncoding? satEncoding;
        Solution? solution;

        try
        {
            instance = model.Instance.Deserialize<Instance>(JsonSerializerOptions) ??
                       throw new InvalidOperationException($"Failed to deserialize instance for problem {model.Id}.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to deserialize problem instance. ProblemId={ProblemId}", model.Id);
            throw;
        }

        try
        {
            satEncoding = model.DimacsEncoding is null
                ? null
                : SatEncoding.Rehydrate(dimacsToSatParser.ParseForSatEncoding(model.DimacsEncoding));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to reconstruct stored SAT encoding. ProblemId={ProblemId}", model.Id);
            throw;
        }

        try
        {
            solution = model.Solution?.Deserialize<Solution>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to deserialize problem solution. ProblemId={ProblemId}", model.Id);
            throw;
        }

        return Problem.Rehydrate(
            model.Id,
            model.Name,
            model.Description,
            model.CreatedAt,
            model.UpdatedAt,
            model.Solver,
            instance,
            satEncoding,
            Enum.Parse<SolvingStatus>(model.SolvingStatus),
            Enum.Parse<Satisfiability>(model.Satisfiability),
            model.Assignment,
            model.StartedSolvingAt,
            model.CompletedAt,
            model.ElapsedTime,
            solution
        );
    }
}
