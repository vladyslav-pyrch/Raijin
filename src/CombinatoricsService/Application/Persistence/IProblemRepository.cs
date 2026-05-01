using Raijin.CombinatoricsService.Application.Features.Problems;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Application.Persistence;

public interface IProblemRepository
{
    public Task<Problem?> GetById(Guid id, CancellationToken cancellationToken);

    public Task<GetProblemResult?> GetSummaryById(Guid id, CancellationToken cancellationToken);

    public Task<GetSatEncodingResult?> GetSatEncodingByProblemId(Guid id, CancellationToken cancellationToken);

    public Task Add(Problem problem, CancellationToken cancellationToken);

    public Task Update(Problem problem, CancellationToken cancellationToken);

    public Task<Problem?> GetOldestPendingWithLock(CancellationToken cancellationToken);

    public Task<ListProblemsResult> ListProblems(int page, int pageSize, CancellationToken cancellationToken);
}