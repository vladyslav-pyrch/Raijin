namespace Raijin.CombinatoricsService.Application.Messaging;

public interface IMessageIdGenerator
{
    public string NextMessageId();
    
    public MessageContext NextMessageContext();
}