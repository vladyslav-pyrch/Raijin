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
            Formula = problem.Formula
        });

        return Task.CompletedTask;
    }

    public Task<BooleanProblem?> GetById(Guid id, CancellationToken cancellationToken)
    {
        BooleanProblemModel? model = dbContext.BooleanProblems
            .FirstOrDefault(problem => problem.Id == id);
        if (model is null)
        {
            logger.LogDebug("Boolean problem {BooleanProblemId} not found in database", id);
            return Task.FromResult<BooleanProblem?>(null);
        }

        BooleanProblem booleanProblem = BooleanProblem.Rehydrate(model.Id, model.Formula);
        logger.LogDebug("Retrieved boolean problem {BooleanProblemId} from database", id);
        return Task.FromResult<BooleanProblem?>(booleanProblem);
    }
}