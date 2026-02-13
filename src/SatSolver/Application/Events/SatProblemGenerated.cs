namespace Raijin.SatSolver.Application.Events;

public record SatProblemGenerated(Guid Id, string Dimacs) : IEvent;