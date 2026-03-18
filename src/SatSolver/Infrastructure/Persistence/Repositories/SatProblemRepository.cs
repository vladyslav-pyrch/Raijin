using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Raijin.SatSolver.Application.Persistence;
using Raijin.SatSolver.Domain.SatProblems;
using Raijin.SatSolver.Infrastructure.Persistence.Models;

namespace Raijin.SatSolver.Infrastructure.Persistence.Repositories;

public sealed class SatProblemRepository(DbContextResolver dbContextResolver, ILogger<SatProblemRepository> logger) : ISatProblemRepository
{
    public Task Add(SatProblem satProblem, CancellationToken cancellationToken)
    {
        logger.LogDebug("Adding SAT problem {SatProblemId} to database", satProblem.Id);
        dbContextResolver.Resolve().SatProblems.Add(new SatProblemModel
        {
            Id = satProblem.Id,
            Dimacs = satProblem.Dimacs,
            Solution = satProblem.Satisfiability switch
            {
                Satisfiability.Satisfiable => satProblem.Solution,
                Satisfiability.Unsatisfiable or Satisfiability.Unknown => [],
                _ => throw new ArgumentOutOfRangeException()
            },
            Satisfiability = satProblem.Satisfiability.ToString()
        });
        return Task.CompletedTask;
    }

    public async Task Update(SatProblem satProblem, CancellationToken cancellationToken)
    {
        logger.LogDebug("Updating SAT problem {SatProblemId}, Satisfiability: {Satisfiability}", satProblem.Id, satProblem.Satisfiability);

        SatSolverDbContext context = dbContextResolver.Resolve();

        SatProblemModel satProblemModel = await context.SatProblems
            .FirstAsync(model => model.Id == satProblem.Id, cancellationToken: cancellationToken);

        satProblemModel.Dimacs = satProblem.Dimacs;
        satProblemModel.Solution = satProblem.Satisfiability switch
        {
            Satisfiability.Satisfiable => satProblem.Solution,
            Satisfiability.Unsatisfiable or Satisfiability.Unknown => [],
            _ => throw new ArgumentOutOfRangeException()
        };
        satProblemModel.Satisfiability = satProblem.Satisfiability.ToString();

        context.SatProblems.Update(satProblemModel);
    }
}