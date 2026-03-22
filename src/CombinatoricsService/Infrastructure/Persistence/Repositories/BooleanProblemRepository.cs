using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Logic;
using Raijin.CombinatoricsService.Infrastructure.Persistence.Models;

namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Repositories;

public class BooleanProblemRepository(
    CombinatoricsServiceDbContext dbContext,
    ILogger<CombinatoricProblemRepository> logger
) : IBooleanProblemRepository
{
    public Task Add(BooleanProblem problem, CancellationToken cancellationToken)
    {
        dbContext.BooleanProblems.Add(new BooleanProblemModel
        {
            Id = problem.Id,
            Formula = problem.Formula,
            Satisfiability = problem.Satisfiability.ToString(),
            Solution = problem.Solution?.Assignments.Select(assignment => new VariableAssignmentModel
            {
                VariableName = assignment.Variable.Name,
                Value = assignment.Value
            }).ToList() ?? []
        });

        return Task.CompletedTask;
    }

    public async Task<BooleanProblem?> GetById(Guid id, CancellationToken cancellationToken)
    {
        BooleanProblemModel? model = await dbContext.BooleanProblems
            .FirstOrDefaultAsync(problem => problem.Id == id, cancellationToken);
        return model is null ? null : BooleanProblem.Rehydrate(model.Id, model.Formula);
    }

    public async Task Update(BooleanProblem problem, CancellationToken cancellationToken)
    {
        BooleanProblemModel? model = await dbContext.BooleanProblems
            .FirstOrDefaultAsync(model => model.Id == problem.Id, cancellationToken);

        if (model is null)
            return;

        model.Formula = problem.Formula;
        model.Satisfiability = problem.Satisfiability.ToString();
        model.Solution = problem.Solution?.Assignments.Select(assignment => new VariableAssignmentModel
        {
            VariableName = assignment.Variable.Name,
            Value = assignment.Value
        }).ToList() ?? [];
        dbContext.BooleanProblems.Update(model);
    }
}