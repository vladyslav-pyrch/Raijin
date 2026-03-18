namespace Raijin.CombinatoricsService.Application.Messaging;

public interface IContextualRequest
{
    public MessageContext Context { get; }
}