using MassTransit;
using Raijin.Application.Contracts;
using Raijin.CombinatoricsService.Infrastructure.Messaging.Payloads;

namespace Raijin.CombinatoricsService.Infrastructure.Messaging.Filters;

public sealed class CausationPublishFilter<TMessage> : IFilter<PublishContext<TMessage>>
    where TMessage : class, IMessage
{
    public async Task Send(PublishContext<TMessage> context, IPipe<PublishContext<TMessage>> next)
    {
        if (context.TryGetPayload(out CausationPayload? payload))
            context.Headers.Set("CausationId", payload.CausationId);

        await next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope(nameof(CausationPublishFilter<>));
    }
}