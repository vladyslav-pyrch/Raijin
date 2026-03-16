using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.CombinatoricProblems;
using Raijin.CombinatoricsService.Infrastructure.Persistence.Models;

namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Repositories;

public class CombinatoricProblemRepository(CombinatoricsServiceDbContext dbContext) : ICombinatoricProblemRepository
{
    public Task Add(CombinatoricProblem problem, CancellationToken cancellationToken)
    {
        var model = new CombinatoricProblemModel
        {
            Id = problem.Id,
            DecisionVariables = problem.DecisionVariables.Select(variable => new DecisionVariableModel
            {
                Name = variable.Name,
                States = variable.States.ToArray()
            }).ToList(),
            Constraints = problem.Constraints.Select(constraint => constraint.Formula).ToArray()
        };
        
        dbContext.CombinatoricProblems.Add(model);
        
        return Task.CompletedTask;
    }
}