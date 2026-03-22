using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.CombinatoricProblems;
using Raijin.CombinatoricsService.Domain.Shared;
using Raijin.CombinatoricsService.Infrastructure.Persistence.Models;

namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Repositories;

public sealed class CombinatoricProblemRepository(
    CombinatoricsServiceDbContext dbContext,
    ILogger<CombinatoricProblemRepository> logger
) : ICombinatoricProblemRepository
{
    public Task Add(CombinatoricProblem problem, CancellationToken cancellationToken)
    {
        dbContext.CombinatoricProblems.Add(new CombinatoricProblemModel
        {
            Id = problem.Id,
            DecisionVariables = problem.DecisionVariables.Select(variable => new DecisionVariableModel
            {
                Name = variable.Name,
                States = variable.States.ToArray()
            }).ToList(),
            Constraints = problem.Constraints.Select(constraint => constraint.Formula).ToArray(),
            Satisfiability = problem.Satisfiability.ToString(),
            Solution = problem.Solution?.Assignments
                .ToDictionary(a => a.DecisionVariable.Name, a => a.SelectedState)
        });

        return Task.CompletedTask;
    }

    public async Task<CombinatoricProblem?> GetById(Guid id, CancellationToken cancellationToken)
    {
        CombinatoricProblemModel? model = await dbContext.CombinatoricProblems
            .FirstOrDefaultAsync(problem => problem.Id == id, cancellationToken);

        if (model is null)
            return null;

        return CombinatoricProblem.Rehydrate(
            model.Id,
            model.DecisionVariables.Select(variable => (variable.Name, States: variable.States.ToArray())).ToArray(),
            model.Constraints.ToArray(),
            Enum.Parse<Satisfiability>(model.Satisfiability),
            model.Solution
        );
    }

    public async Task Update(CombinatoricProblem problem, CancellationToken cancellationToken)
    {
        CombinatoricProblemModel? model = await dbContext.CombinatoricProblems
            .FirstOrDefaultAsync(m => m.Id == problem.Id, cancellationToken);

        if (model is null)
            throw new InvalidOperationException($"CombinatoricProblem '{problem.Id}' was not found in the database.");

        model.DecisionVariables = problem.DecisionVariables.Select(variable => new DecisionVariableModel
        {
            Name = variable.Name,
            States = variable.States.ToArray()
        }).ToList();
        model.Constraints = problem.Constraints.Select(constraint => constraint.Formula).ToArray();
        model.Satisfiability = problem.Satisfiability.ToString();
        model.Solution = problem.Solution?.Assignments
            .ToDictionary(a => a.DecisionVariable.Name, a => a.SelectedState);
    }
}