using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Infrastructure.Messaging;

public sealed class GuidMessageIdGenerator : IMessageIdGenerator
{
    public string NextMessageId() => Guid.CreateVersion7().ToString();

    public MessageContext NextMessageContext()  
    {
        string messageId = NextMessageId();
        return new MessageContext(messageId, messageId);
    }
}