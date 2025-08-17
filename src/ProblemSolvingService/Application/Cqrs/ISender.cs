namespace Raijin.ProblemSolvingService.Application.Cqrs;

public interface ISender
{
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

    public Task Send(IRequest request, CancellationToken cancellationToken);
}