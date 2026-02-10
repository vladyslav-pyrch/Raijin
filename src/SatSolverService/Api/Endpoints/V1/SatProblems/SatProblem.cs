namespace Raijin.SatSolverService.Api.Endpoints.V1.SatProblems;

public class SatProblem
{
    public int Id { get; set; }

    public string Dimacs { get; set; }

    public SatProblemStatus Status { get; set; }

    public string? Result { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }
}