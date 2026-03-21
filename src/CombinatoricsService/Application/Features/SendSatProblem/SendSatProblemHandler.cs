using FluentResults;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Application.Features.SendSatProblem;

public sealed class SendSatProblemHandler(
    IMessageBus messageBus,
    IMessageIdGenerator messageIdGenerator,
    IMessageContextAccessor messageContextAccessor,
    ILogger<SendSatProblemHandler> logger
) : IRequestHandler<SendSatProblemCommand, SendSatProblemResult>
{
    public async Task<Result<SendSatProblemResult>> Handle(
        SendSatProblemCommand request,
        CancellationToken cancellationToken)
    {
        await messageBus.Publish<ISatProblemSent>(new
        {
            MessageId = messageIdGenerator.NextMessageId(),
            messageContextAccessor.CurrentContext.CorrelationId,
            messageContextAccessor.CurrentContext.CausationId,
            Timestamp = DateTimeOffset.UtcNow,
            SatProblemId = request.SatProblemId.ToString(),
            request.Dimacs
        }, cancellationToken);
        return new SendSatProblemResult(request.SatProblemId);
    }
}