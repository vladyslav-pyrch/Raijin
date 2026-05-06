namespace Raijin.CombinatoricsService.Application.Persistence;

public interface IUnitOfWork
{
    public Task BeginTransaction(CancellationToken cancellationToken);

    public Task Commit(CancellationToken cancellationToken);
}