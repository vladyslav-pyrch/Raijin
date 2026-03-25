using MassTransit;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;
using Raijin.SatSolver.Application.Messaging;

namespace Raijin.SatSolver.Infrastructure.Messaging.Filters;

public sealed class LoggingConsumeFilter<TMessage>(
    ICorrelationContextAccessor accessor,
    ILogger<LoggingConsumeFilter<TMessage>> logger
) : IFilter<ConsumeContext<TMessage>> where TMessage : class, IMessage
{
    private readonly string MessageType = typeof(TMessage).Name;

    public async Task Send(ConsumeContext<TMessage> context, IPipe<ConsumeContext<TMessage>> next)
    {
        using IDisposable? scope = logger.BeginScope(
            "InitiatorId: {InitiatorId}, CorrelationId: {CorrelationId}, UserId: {UserId}",
            accessor.CorrelationContext.InitiatorId,
            accessor.CorrelationContext.CorrelationId,
            accessor.CorrelationContext.UserId
        );

        logger.LogInformation(
            "Consuming message of type {MessageType} with MessageId: {MessageId} caused by CausationId: {CausationId}",
            MessageType,
            context.MessageId,
            context.Headers.Get<string>("CausationId")
        );

        try
        {
            await next.Send(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error occurred while handling {MessageType}", MessageType);
            throw;
        }

        logger.LogInformation("Finished consuming {MessageType}", MessageType);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope(nameof(LoggingConsumeFilter<>));
    }
}