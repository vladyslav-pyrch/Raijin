using Microsoft.Extensions.Logging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.CombinatoricProblems;
using Raijin.CombinatoricsService.Infrastructure.Persistence.Models;

namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Repositories;

public sealed class CombinatoricProblemRepository(
    DbContextResolver dbContextResolver,
    ILogger<CombinatoricProblemRepository> logger
) : ICombinatoricProblemRepository
{
    public Task Add(CombinatoricProblem problem, CancellationToken cancellationToken)
    {
        logger.LogDebug("Adding combinatoric problem {CombinatoricProblemId} to database", problem.Id);
        
        dbContextResolver.Resolve().CombinatoricProblems.Add(new CombinatoricProblemModel
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

    public Task<CombinatoricProblem?> GetById(Guid id, CancellationToken cancellationToken)
    {
        logger.LogDebug("Retrieving combinatoric problem {CombinatoricProblemId} from database", id);

        CombinatoricsServiceDbContext context = dbContextResolver.Resolve();

        CombinatoricProblemModel? model = context.CombinatoricProblems
            .FirstOrDefault(problem => problem.Id == id);

        if (model is null)
        {
            logger.LogDebug("Combinatoric problem {CombinatoricProblemId} not found in database", id);
            return Task.FromResult<CombinatoricProblem?>(null);
        }

        var combinatoricProblem = new CombinatoricProblem(model.Id);

        foreach (DecisionVariableModel variableModel in model.DecisionVariables)
            combinatoricProblem.AddDecisionVariable(variableModel.Name, variableModel.States);

        foreach (string constraint in model.Constraints) 
            combinatoricProblem.AddConstrain(constraint);

        logger.LogDebug("Retrieved combinatoric problem {CombinatoricProblemId} with {VariableCount} variables and {ConstraintCount} constraints",
            id, model.DecisionVariables.Count, model.Constraints.Length);
        return Task.FromResult<CombinatoricProblem?>(combinatoricProblem);
    }

}