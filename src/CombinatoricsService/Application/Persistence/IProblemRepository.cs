using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Application.Persistence;

public interface IProblemRepository
{
    public Task<Problem?> GetById(Guid id, CancellationToken cancellationToken);

    public Task Add(Problem problem, CancellationToken cancellationToken);

    public Task Update(Problem problem, CancellationToken cancellationToken);

    public Task<Problem?> GetOldestPendingWithLock(CancellationToken cancellationToken);

    public Task<(IReadOnlyList<Problem> Items, int TotalCount)> GetPage(int page, int pageSize, CancellationToken cancellationToken);
}