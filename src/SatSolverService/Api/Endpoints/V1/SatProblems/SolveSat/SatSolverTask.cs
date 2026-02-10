namespace Raijin.SatSolverService.Api.Endpoints.V1.SatProblems.SolveSat;

/// <summary>
/// Represents a background task to be executed by the background worker.
/// </summary>
public record SatSolverTask(int SatProblemId, string Dimacs, int? Timeout);