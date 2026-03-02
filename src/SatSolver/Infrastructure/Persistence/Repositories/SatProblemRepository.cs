using Microsoft.EntityFrameworkCore;
using Raijin.SatSolver.Application.Persistence;
using Raijin.SatSolver.Domain.SatProblems;
using Raijin.SatSolver.Infrastructure.Persistence.Models;

namespace Raijin.SatSolver.Infrastructure.Persistence.Repositories;

public class SatProblemRepository(SatSolverDbContext dbContext) : ISatProblemRepository
{
    public Task Add(SatProblem satProblem, CancellationToken cancellationToken)
    {
        var satProblemModel =  new SatProblemModel
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
        };

        dbContext.SatProblems.Add(satProblemModel);
        
        return Task.CompletedTask; 
    }

    public async Task Update(SatProblem satProblem, CancellationToken cancellationToken)
    {
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

    public async Task Save(CancellationToken cancellationToken) => await dbContext.SaveChangesAsync(cancellationToken);
}