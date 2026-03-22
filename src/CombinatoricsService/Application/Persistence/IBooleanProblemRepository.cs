using Raijin.CombinatoricsService.Domain.Logic;

namespace Raijin.CombinatoricsService.Application.Persistence;

public interface IBooleanProblemRepository
{
    public Task Add(BooleanProblem problem, CancellationToken cancellationToken);

    public Task<BooleanProblem?> GetById(Guid id, CancellationToken cancellationToken);

    public Task Update(BooleanProblem problem, CancellationToken cancellationToken);
}