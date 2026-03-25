using Raijin.CombinatoricsService.Domain.BooleanProblems;

namespace Raijin.CombinatoricsService.Application.Persistence;

public interface IBooleanProblemRepository
{
    public Task Add(BooleanProblem problem, CancellationToken cancellationToken);

    public Task<BooleanProblem?> GetById(Guid id, CancellationToken cancellationToken);

    public Task Update(BooleanProblem problem, CancellationToken cancellationToken);
}