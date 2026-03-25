using Raijin.CombinatoricsService.Domain.CombinatoricProblems;

namespace Raijin.CombinatoricsService.Application.Persistence;

public interface ICombinatoricProblemRepository
{
    public Task Add(CombinatoricProblem problem, CancellationToken cancellationToken);

    public Task<CombinatoricProblem?> GetById(Guid id, CancellationToken cancellationToken);

    public Task Update(CombinatoricProblem problem, CancellationToken cancellationToken);
}