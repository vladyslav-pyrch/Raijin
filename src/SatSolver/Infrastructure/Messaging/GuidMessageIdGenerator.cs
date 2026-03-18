using Raijin.SatSolver.Application.Messaging;

namespace Raijin.SatSolver.Infrastructure.Messaging;

public sealed class GuidMessageIdGenerator : IMessageIdGenerator
{
    public string NextMessageId() => Guid.CreateVersion7().ToString();

    public MessageContext NextMessageContext()  
    {
        string messageId = NextMessageId();
        return new MessageContext(messageId, messageId);
    }
}