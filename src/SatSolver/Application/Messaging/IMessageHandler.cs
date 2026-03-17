using Raijin.Application.Contracts;

namespace Raijin.SatSolver.Application.Messaging;

public interface IMessageHandler<in TEvent> where TEvent : class, IMessage
{
    public Task Handle(TEvent @event, CancellationToken cancellationToken);
}