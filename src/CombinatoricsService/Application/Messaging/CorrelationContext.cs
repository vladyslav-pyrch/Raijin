namespace Raijin.CombinatoricsService.Application.Messaging;

public sealed record CorrelationContext(
    Guid? InitiatorId,
    Guid? CorrelationId,
    string? UserId
);