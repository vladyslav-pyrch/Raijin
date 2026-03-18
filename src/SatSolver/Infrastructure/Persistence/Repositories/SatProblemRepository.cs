using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Raijin.SatSolver.Application.Persistence;
using Raijin.SatSolver.Domain.SatProblems;
using Raijin.SatSolver.Infrastructure.Persistence.Models;

namespace Raijin.SatSolver.Infrastructure.Persistence.Repositories;

public sealed class SatProblemRepository(SatSolverDbContext dbContext, ILogger<SatProblemRepository> logger) : ISatProblemRepository
{
    public Task Add(SatProblem satProblem, CancellationToken cancellationToken)
    {
        logger.LogDebug("Adding SAT problem {SatProblemId} to database", satProblem.Id);
        dbContext.SatProblems.Add(new SatProblemModel
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

    public async Task<SatProblem?> GetById(Guid id, CancellationToken cancellationToken)
    {
        logger.LogDebug("Retrieving SAT problem {SatProblemId} from database", id);

        SatProblemModel? model = await dbContext.SatProblems
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (model is null)
        {
            logger.LogDebug("SAT problem {SatProblemId} not found in database", id);
            return null;
        }

        Satisfiability satisfiability = Enum.Parse<Satisfiability>(model.Satisfiability);

        var satProblem = SatProblem.Rehydrate(model.Id, model.Dimacs, satisfiability, model.Solution);

        return satProblem;
    }

    public async Task Update(SatProblem satProblem, CancellationToken cancellationToken)
    {
        logger.LogDebug("Updating SAT problem {SatProblemId}, Satisfiability: {Satisfiability}", satProblem.Id, satProblem.Satisfiability);

        SatProblemModel satProblemModel = await dbContext.SatProblems
            .FirstAsync(model => model.Id == satProblem.Id, cancellationToken: cancellationToken);

        satProblemModel.Dimacs = satProblem.Dimacs;
        satProblemModel.Solution = satProblem.Satisfiability switch
        {
            Satisfiability.Satisfiable => satProblem.Solution,
            Satisfiability.Unsatisfiable or Satisfiability.Unknown => [],
            _ => throw new ArgumentOutOfRangeException()
        };
        satProblemModel.Satisfiability = satProblem.Satisfiability.ToString();

        dbContext.SatProblems.Update(satProblemModel);
    }
}