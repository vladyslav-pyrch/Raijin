namespace Raijin.SatSolver.Application.Events;

public record SatProblemSolved(Guid Id, int[] Solution): IEvent;