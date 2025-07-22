namespace Raijin.ProblemSolvingService.Application.Cqrs;

public interface IQueryDispatcher
{
    public Task<TResult> Dispatch<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default) where TQuery : IQuery<TResult>;
}