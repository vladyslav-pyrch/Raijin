using MassTransit;
using Raijin.Application.Contracts;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Infrastructure.Messaging.Filters;

public sealed class CorrelationContextSendFilter<TMessage>(
    ICorrelationContextAccessor correlationContextAccessor
) : IFilter<SendContext<TMessage>>
    where TMessage : class, IMessage
{
    public async Task Send(SendContext<TMessage> context, IPipe<SendContext<TMessage>> next)
    {
        context.InitiatorId = correlationContextAccessor.CorrelationContext.InitiatorId;
        context.CorrelationId = correlationContextAccessor.CorrelationContext.CorrelationId;

        if (correlationContextAccessor.CorrelationContext.UserId is not null)
            context.Headers.Set("UserId", correlationContextAccessor.CorrelationContext.UserId);

        await next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope(nameof(CorrelationContextSendFilter<>));
    }
}