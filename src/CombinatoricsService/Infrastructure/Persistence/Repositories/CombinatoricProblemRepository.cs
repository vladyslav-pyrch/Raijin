using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.CombinatoricProblems;
using Raijin.CombinatoricsService.Infrastructure.Persistence.Models;

namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Repositories;

public sealed class CombinatoricProblemRepository(
    CombinatoricsServiceDbContext dbContext,
    ILogger<CombinatoricProblemRepository> logger
) : ICombinatoricProblemRepository
{
    public Task Add(CombinatoricProblem problem, CancellationToken cancellationToken)
    {
        logger.LogDebug("Adding combinatoric problem {CombinatoricProblemId} to database", problem.Id);
        
        dbContext.CombinatoricProblems.Add(new CombinatoricProblemModel
        {
            Id = problem.Id,
            DecisionVariables = problem.DecisionVariables.Select(variable => new DecisionVariableModel
            {
                Name = variable.Name,
                States = variable.States.ToArray()
            }).ToList(),
            Constraints = problem.Constraints.Select(constraint => constraint.Formula).ToArray()
        });

        return Task.CompletedTask;
    }

    public async Task<CombinatoricProblem?> GetById(Guid id, CancellationToken cancellationToken)
    {
        logger.LogDebug("Retrieving combinatoric problem {CombinatoricProblemId} from database", id);

        CombinatoricProblemModel? model = await dbContext.CombinatoricProblems
            .FirstOrDefaultAsync(problem => problem.Id == id, cancellationToken: cancellationToken);

        if (model is null)
        {
            logger.LogDebug("Combinatoric problem {CombinatoricProblemId} not found in database", id);
            return null;
        }

        CombinatoricProblem combinatoricProblem = CombinatoricProblem.Rehydrate(
            model.Id,
            model.DecisionVariables.Select(variable => (variable.Name, States: variable.States.ToArray())).ToArray(),
            model.Constraints.ToArray()
        );

        logger.LogDebug("Retrieved combinatoric problem {CombinatoricProblemId} with {VariableCount} variables and {ConstraintCount} constraints",
            id, model.DecisionVariables.Count, model.Constraints.Length);
        return combinatoricProblem;
    }

}