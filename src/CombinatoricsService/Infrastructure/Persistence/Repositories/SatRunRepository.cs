 using Microsoft.EntityFrameworkCore;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.SatRuns;
using Raijin.CombinatoricsService.Domain.Shared;
using Raijin.CombinatoricsService.Infrastructure.Persistence.Models;

namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Repositories;

public class SatRunRepository(CombinatoricsServiceDbContext dbContext) : ISatRunRepository
{
    public async Task<SatRun?> GetById(Guid id, CancellationToken cancellationToken)
    {
        var model = await dbContext.SatRuns
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return model is null ? null : ToDomain(model);
    }

    public async Task<IReadOnlyList<SatRun>> GetByProblemId(Guid problemId, CancellationToken cancellationToken)
    {
        var models = await dbContext.SatRuns
            .Where(x => x.ProblemId == problemId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return models.Select(ToDomain).ToList();
    }

    public Task Add(SatRun satRun, Guid problemId, CancellationToken cancellationToken)
    {
        dbContext.SatRuns.Add(ToModel(satRun, problemId));
        return Task.CompletedTask;
    }

    public async Task Update(SatRun satRun, CancellationToken cancellationToken)
    {
        var existingModel = await dbContext.SatRuns.FindAsync([satRun.Id], cancellationToken);

        if (existingModel is null)
            throw new InvalidOperationException($"SatRun {satRun.Id} not found.");

        existingModel.Status = satRun.Status.ToString();
        existingModel.Satisfiability = satRun.Satisfiability.ToString();
        existingModel.Assignment = satRun.Assignment.ToArray();
        existingModel.CompletedAt = satRun.CompletedAt;
    }

    public async Task<SatRun?> GetOldestPendingWithLock(CancellationToken cancellationToken)
    {
        var pending = nameof(SatRunStatus.Pending);

        var model = await dbContext.SatRuns
            .FromSql(
                $"""
                SELECT * FROM "SatRuns"
                WHERE "Status" = {pending}
                ORDER BY "CreatedAt" ASC
                LIMIT 1
                FOR UPDATE SKIP LOCKED
                """)
            .Include(s => s.SatEncoding)
            .ThenInclude(e => e.Clauses)
            .FirstOrDefaultAsync(cancellationToken);

        return model is null ? null : ToDomain(model);
    }

    private static SatRunModel ToModel(SatRun satRun, Guid problemId) => new()
    {
        Id = satRun.Id,
        ProblemId = problemId,
        Status = satRun.Status.ToString(),
        Satisfiability = satRun.Satisfiability.ToString(),
        Assignment = satRun.Assignment.ToArray(),
        CreatedAt = satRun.CreatedAt,
        CompletedAt = satRun.CompletedAt,
        SatEncoding = new SatEncodingModel
        {
            ProblemId = problemId,
            Clauses = satRun.SatEncoding.Clauses
                .Select(clause => new ClauseModel
                {
                    Literals = clause.ToArray()
                })
                .ToList()
        }
    };

    private static SatRun ToDomain(SatRunModel model) =>
        SatRun.Rehydrate(
            model.Id,
            SatEncoding.Rehydrate(model.SatEncoding.Clauses.Select(c => (IEnumerable<int>)c.Literals)),
            Enum.Parse<SatRunStatus>(model.Status),
            Enum.Parse<Satisfiability>(model.Satisfiability),
            model.Assignment,
            model.CreatedAt,
            model.CompletedAt
        );
}
