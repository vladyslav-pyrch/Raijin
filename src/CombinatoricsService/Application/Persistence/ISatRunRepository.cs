using Raijin.CombinatoricsService.Domain.SatRuns;

namespace Raijin.CombinatoricsService.Application.Persistence;

public interface ISatRunRepository
{
  public Task<SatRun?> GetById(Guid id, CancellationToken cancellationToken);

  public Task<IReadOnlyList<SatRun>> GetByProblemId(Guid problemId, CancellationToken cancellationToken);

  public Task Add(SatRun satRun, Guid problemId, CancellationToken cancellationToken);

  public Task Update(SatRun satRun, CancellationToken cancellationToken);

  public Task<SatRun?> GetOldestPendingWithLock(CancellationToken cancellationToken);
}
