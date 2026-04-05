namespace Raijin.SatSolver.Domain.SatProblems;

public record Solution(IEnumerable<int> Assignments, Satisfiability Satisfiability)
{
    public static Solution Unknown() => new([], Satisfiability.Unknown);

    public static Solution Unsatisfiable() => new([], Satisfiability.Unsatisfiable);

    public static Solution Satisfiable(IEnumerable<int> assignments)
    {
        int[] enumerable = assignments as int[] ?? assignments.ToArray();

        if (!enumerable.Any())
            throw new ArgumentException("Assignments of satisfiable solution may not be empty", nameof(assignments));

        return new Solution(enumerable, Satisfiability.Satisfiable);
    }
}