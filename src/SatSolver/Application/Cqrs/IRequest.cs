namespace Raijin.SatSolver.Application.Cqrs;

public interface IRequest;

public interface IRequest<TResponse> : IRequest;