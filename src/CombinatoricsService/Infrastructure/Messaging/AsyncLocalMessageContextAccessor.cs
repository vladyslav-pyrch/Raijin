using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Infrastructure.Messaging;

public sealed class AsyncLocalMessageContextAccessor : IMessageContextAccessor
{
    private static readonly AsyncLocal<MessageContext> Context = new();

    public MessageContext CurrentContext
    {
        get => Context.Value ?? throw new InvalidOperationException("No message context is currently set.");
        set => Context.Value = value;
    }
}