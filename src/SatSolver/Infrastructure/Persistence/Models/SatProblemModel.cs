namespace Raijin.SatSolver.Infrastructure.Persistence.Models;

internal class SatProblemModel
{
    public Guid Id { get; set; }

    public string Dimacs { get; set; } = null!;

    public int[] Solution { get; set; } = [];

    public string Satisfiability { get; set; } = null!;
}