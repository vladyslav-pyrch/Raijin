using MassTransit;
using Microsoft.Extensions.Logging;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Infrastructure.Messaging;

internal sealed class CorrelationContextConsumeFilter<T>(
    ICorrelationContextAccessor accessor,
    ILogger<CorrelationContextConsumeFilter<T>> logger
) : IFilter<ConsumeContext<T>> where T : class
{
    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        Guid correlationId = context.CorrelationId ??
                             throw new InvalidOperationException("All messages should have \"CorrelationId\" set");
        Guid causationId = context.InitiatorId ?? correlationId;

        accessor.CorrelationContext = new CorrelationContext(correlationId, causationId, null);

        using IDisposable? scope = logger.BeginScope(new Dictionary<string, object?>
        {
            ["CorrelationId"] = correlationId,
            ["CausationId"] = causationId
        });

        await next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("correlationContext");
    }
}