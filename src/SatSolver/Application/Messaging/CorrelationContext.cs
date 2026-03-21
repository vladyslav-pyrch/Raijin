namespace Raijin.SatSolver.Application.Messaging;

public sealed record CorrelationContext(Guid CorrelationId, Guid? CausationId, string? UserId);