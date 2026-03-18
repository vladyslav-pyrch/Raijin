namespace Raijin.CombinatoricsService.Application.Persistence;

public interface IUnitOfWork
{
    public Task SaveChanges(CancellationToken cancellationToken);
}