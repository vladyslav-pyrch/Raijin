namespace Raijin.CombinatoricsService.Application.Persistence;

public interface IUnitOfWork
{
    public Task Commit(CancellationToken cancellationToken);
}