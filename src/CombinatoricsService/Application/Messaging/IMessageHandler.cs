namespace Raijin.CombinatoricsService.Application.Messaging;

public interface IMessageHandler<in TEvent> where TEvent : class
{
    public Task Handle(TEvent @event, CancellationToken cancellationToken);
}