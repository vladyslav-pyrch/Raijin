using MassTransit;
using Raijin.Application.Contracts;
using Raijin.CombinatoricsService.Infrastructure.Messaging.Payloads;

namespace Raijin.CombinatoricsService.Infrastructure.Messaging.Filters;

public sealed class CausationConsumeFilter<TMessage> : IFilter<ConsumeContext<TMessage>>
    where TMessage : class, IMessage
{
    public async Task Send(ConsumeContext<TMessage> context, IPipe<ConsumeContext<TMessage>> next)
    {
        if (context.MessageId.HasValue)
            context.GetOrAddPayload(() => new CausationPayload(context.MessageId.Value));

        await next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope(nameof(CausationConsumeFilter<>));
    }
}