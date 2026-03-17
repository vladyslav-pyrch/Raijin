using Raijin.Application.Contracts;

namespace Raijin.CombinatoricsService.Application.Messaging;

public sealed record MessageContext(string CorrelationId, string CausationId)
{
    public MessageContext(IMessage original) : this(original.CorrelationId, original.MessageId)
    { }
}