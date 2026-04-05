using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Raijin.SatSolver.Application.Persistence;
using Raijin.SatSolver.Domain.SatProblems;
using Raijin.SatSolver.Infrastructure.Persistence.Models;

namespace Raijin.SatSolver.Infrastructure.Persistence.Repositories;

public sealed class SatProblemRepository(SatSolverDbContext dbContext, ILogger<SatProblemRepository> logger)
    : ISatProblemJobRepository
{
    public Task Add(SatProblem satProblem, CancellationToken cancellationToken)
    {
        dbContext.SatProblems.Add(new SatProblemModel
        {
            Id = satProblem.Id,
            Solution = satProblem.Solution.Assignments.ToArray(),
            Satisfiability = satProblem.Solution.Satisfiability.ToString(),
            SolvingStatus = satProblem.SolvingStatus.ToString(),
            Clauses = satProblem.Clauses.Select(clause => new ClauseModel
            {
                Literals = clause.Literals.Select(literal => literal.Value).ToArray()
            }).ToList(),
            CreatedAt = DateTime.UtcNow
        });

        return Task.CompletedTask;
    }

    public async Task<SatProblem?> GetById(Guid id, CancellationToken cancellationToken)
    {
        SatProblemModel? model = await dbContext.SatProblems.FindAsync([id], cancellationToken);

        if (model is null)
            return null;

        return ModelToSatProblem(model);
    }

    public async Task Update(SatProblem satProblem, CancellationToken cancellationToken)
    {
        SatProblemModel? model = await dbContext.SatProblems.FindAsync([satProblem.Id], cancellationToken);

        if (model is null)
            throw new InvalidOperationException("Trying to update sat problem that is not persisted yet.");

        model.Solution = satProblem.Solution.Assignments.ToArray();
        model.Satisfiability = satProblem.Solution.Satisfiability.ToString();
        model.SolvingStatus = satProblem.SolvingStatus.ToString();
        model.Clauses = satProblem.Clauses.Select(clause => new ClauseModel
        {
            Literals = clause.Literals.Select(literal => literal.Value).ToArray()
        }).ToList();
    }

    public async Task<SatProblem?> GetNextPendingAndLock(CancellationToken cancellationToken)
    {
        SatProblemModel? model = await dbContext.SatProblems
            .FromSqlRaw("""
                        SELECT * FROM "SatProblems"
                        WHERE "SolvingStatus" = 'Pending' 
                        ORDER BY "CreatedAt"
                        LIMIT 1 
                        FOR UPDATE SKIP LOCKED
                        """)
            .Include(model => model.Clauses)
            .AsTracking() // Ensure it's tracked so UnitOfWork can save the domain changes
            .FirstOrDefaultAsync(cancellationToken);

        if (model is null)
            return null;

        return ModelToSatProblem(model);
    }

    private static SatProblem ModelToSatProblem(SatProblemModel model) => SatProblem.Rehydrate(
        model.Id,
        Enum.Parse<SolvingStatus>(model.SolvingStatus),
        model.Clauses.Select(clause => clause.Literals),
        Enum.Parse<Satisfiability>(model.Satisfiability),
        model.Solution
    );
}