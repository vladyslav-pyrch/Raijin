using FluentResults;

namespace Raijin.SatSolver.Application.Messaging;

public interface IMediator
{
    public Task<Result> Send(IRequest request, CancellationToken cancellationToken);

    public Task<Result<TResponse>> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken);
}