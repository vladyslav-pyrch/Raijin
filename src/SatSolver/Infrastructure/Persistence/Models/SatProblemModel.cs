namespace Raijin.SatSolver.Infrastructure.Persistence.Models;

internal class SatProblemModel
{
    public Guid Id { get; set; }

    public int[] Solution { get; set; } = [];

    public string Satisfiability { get; set; } = null!;

    public string SolvingStatus { get; set; } = null!;

    public ICollection<ClauseModel> Clauses { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}