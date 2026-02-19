namespace Raijin.SatSolver.Infrastructure.Messaging;

public interface IConsumer
{
    public Task Start(CancellationToken cancellationToken);
}