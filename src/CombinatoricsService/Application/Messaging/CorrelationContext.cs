namespace Raijin.CombinatoricsService.Application.Messaging;

public sealed record CorrelationContext(Guid CorrelationId, Guid? CausationId, string? UserId);