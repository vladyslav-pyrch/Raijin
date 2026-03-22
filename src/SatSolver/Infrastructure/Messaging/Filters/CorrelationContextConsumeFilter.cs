using MassTransit;
using Raijin.Application.Contracts;
using Raijin.SatSolver.Application.Messaging;

namespace Raijin.SatSolver.Infrastructure.Messaging.Filters;

public sealed class CorrelationContextConsumeFilter<TMessage>(
    ICorrelationContextAccessor accessor
) : IFilter<ConsumeContext<TMessage>> where TMessage : class, IMessage
{
    public async Task Send(ConsumeContext<TMessage> context, IPipe<ConsumeContext<TMessage>> next)
    {
        var userId = context.Headers.Get<string>("UserId");

        accessor.CorrelationContext = new CorrelationContext(
            context.InitiatorId,
            context.CorrelationId,
            userId
        );

        await next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope(nameof(CorrelationContextConsumeFilter<>));
    }
}