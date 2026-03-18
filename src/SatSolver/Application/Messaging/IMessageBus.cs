using Raijin.Application.Contracts;

namespace Raijin.SatSolver.Application.Messaging;

public interface IMessageBus
{
    public Task Publish<TMessage>(object message, CancellationToken cancellationToken) where TMessage : class, IMessage;
}