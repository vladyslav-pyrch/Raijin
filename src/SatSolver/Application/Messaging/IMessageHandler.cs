using Raijin.Application.Contracts;

namespace Raijin.SatSolver.Application.Messaging;

public interface IMessageHandler<in TMessage> where TMessage : class, IMessage
{
    public Task Handle(TMessage message, CancellationToken cancellationToken);
}