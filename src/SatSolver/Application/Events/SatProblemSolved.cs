namespace Raijin.SatSolver.Application.Events;

[EventMetadata("sat-problem.solved")]
public record SatProblemSolved(Guid Id, int[] Solution): IEvent;