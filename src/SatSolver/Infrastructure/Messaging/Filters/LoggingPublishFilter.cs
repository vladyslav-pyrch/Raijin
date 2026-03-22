using MassTransit;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;

namespace Raijin.SatSolver.Infrastructure.Messaging.Filters;

public sealed class LoggingPublishFilter<TMessage>(
    ILogger<LoggingPublishFilter<TMessage>> logger
) : IFilter<PublishContext<TMessage>> where TMessage : class, IMessage
{
    public async Task Send(PublishContext<TMessage> context, IPipe<PublishContext<TMessage>> next)
    {
        string messageType = context.Message.GetType().Name;

        logger.LogInformation(
            "Publishing message of type {MessageType} with MessageId: {MessageId} and Body: {@Message}", messageType,
            context.MessageId, context.Message);

        await next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope(nameof(LoggingPublishFilter<>));
    }
}