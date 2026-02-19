namespace Raijin.SatSolver.Application.Events;

[EventMetadata("sat-problem.generated")]
public record SatProblemGenerated(Guid Id, string Dimacs) : IEvent;