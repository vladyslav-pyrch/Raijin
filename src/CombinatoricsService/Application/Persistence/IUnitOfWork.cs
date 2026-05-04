namespace Raijin.CombinatoricsService.Application.Persistence;

/// <summary>
///     Write-only unit of work. Call <see cref="Commit" /> after all repository
///     mutations to flush pending changes to the store.
/// </summary>
public interface IUnitOfWork
{
    public Task BeginTransaction(CancellationToken cancellationToken);

    public Task Commit(CancellationToken cancellationToken);
}