using FluentResults;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Application.Features.SendSatProblem;

public sealed class SendSatProblemHandler(
    IMessageBus messageBus,
    ICorrelationContextAccessor correlationContextAccessor,
    ILogger<SendSatProblemHandler> logger
) : IRequestHandler<SendSatProblemCommand, SendSatProblemResult>
{
    public async Task<Result<SendSatProblemResult>> Handle(
        SendSatProblemCommand request,
        CancellationToken cancellationToken)
    {
        await messageBus.Publish<ISatProblemSent>(new
        {
            SatProblemId = request.SatProblemId.ToString(),
            request.Dimacs,
            correlationContextAccessor.CorrelationContext.CorrelationId
        }, cancellationToken);
        return new SendSatProblemResult(request.SatProblemId);
    }
}