using Raijin.Application.Contracts;

namespace Raijin.CombinatoricsService.Application.Messaging;

public interface IMessageBus
{
    public Task Publish<TMessage>(object message, CancellationToken cancellationToken) where TMessage : class, IMessage;

    public Task Send<TMessage>(object message, CancellationToken cancellationToken) where TMessage : class, IMessage;
}