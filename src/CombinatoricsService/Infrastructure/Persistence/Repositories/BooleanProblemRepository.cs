using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.BooleanProblems;
using Raijin.CombinatoricsService.Domain.Shared;
using Raijin.CombinatoricsService.Infrastructure.Persistence.Models;

namespace Raijin.CombinatoricsService.Infrastructure.Persistence.Repositories;

public sealed class BooleanProblemRepository(
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
            Solution = problem.Solution.Assignments.ToDictionary(
                assignment => assignment.Variable.Name,
                assignment => assignment.Value
            )
        });

        return Task.CompletedTask;
    }

    public async Task<BooleanProblem?> GetById(Guid id, CancellationToken cancellationToken)
    {
        BooleanProblemModel? model = await dbContext.BooleanProblems
            .FirstOrDefaultAsync(problem => problem.Id == id, cancellationToken);

        if (model is null)
            return null;

        return BooleanProblem.Rehydrate(
            model.Id,
            model.Formula,
            Enum.Parse<Satisfiability>(model.Satisfiability),
            model.Solution
        );
    }

    public async Task Update(BooleanProblem problem, CancellationToken cancellationToken)
    {
        BooleanProblemModel? model = await dbContext.BooleanProblems
            .FirstOrDefaultAsync(model => model.Id == problem.Id, cancellationToken);

        if (model is null)
            return;

        model.Formula = problem.Formula;
        model.Satisfiability = problem.Satisfiability.ToString();
        model.Solution = problem.Solution.Assignments.ToDictionary(
            assignment => assignment.Variable.Name,
            assignment => assignment.Value
        );
    }
}