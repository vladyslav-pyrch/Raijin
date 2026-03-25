using MassTransit;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;

namespace Raijin.CombinatoricsService.Infrastructure.Messaging.Filters;

public sealed class LoggingPublishFilter<TMessage>(
    ILogger<LoggingPublishFilter<TMessage>> logger
) : IFilter<PublishContext<TMessage>> where TMessage : class, IMessage
{
    private readonly string MessageType = typeof(TMessage).Name;

    public async Task Send(PublishContext<TMessage> context, IPipe<PublishContext<TMessage>> next)
    {
        logger.LogInformation(
            "Publishing message of type {MessageType} with MessageId: {MessageId} and Body: {@Message}", MessageType,
            context.MessageId, context.Message);

        await next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope(nameof(LoggingPublishFilter<>));
    }
}