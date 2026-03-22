using MassTransit;
using Raijin.Application.Contracts;
using Raijin.SatSolver.Application.Messaging;

namespace Raijin.SatSolver.Infrastructure.Messaging.Filters;

public sealed class CorrelationContextPublishFilter<TMessage>(
    ICorrelationContextAccessor correlationContextAccessor
) : IFilter<PublishContext<TMessage>>
    where TMessage : class, IMessage
{
    public async Task Send(PublishContext<TMessage> context, IPipe<PublishContext<TMessage>> next)
    {
        context.InitiatorId = correlationContextAccessor.CorrelationContext.InitiatorId;
        context.CorrelationId = correlationContextAccessor.CorrelationContext.CorrelationId;

        if (correlationContextAccessor.CorrelationContext.UserId is not null)
            context.Headers.Set("UserId", correlationContextAccessor.CorrelationContext.UserId);

        await next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope(nameof(CorrelationContextPublishFilter<>));
    }
}