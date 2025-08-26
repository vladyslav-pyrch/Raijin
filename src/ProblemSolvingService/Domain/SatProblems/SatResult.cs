namespace Raijin.ProblemSolvingService.Domain.SatProblems;

public sealed record SatResult
{
    private SatResult(SolvingStatus status, IReadOnlyList<VariableAssignment> assignments)
    {
        Status = status;
        Assignments = assignments;
    }

    public SolvingStatus Status { get; }

    public IReadOnlyList<VariableAssignment> Assignments { get; }

    public static SatResult Solvable(IReadOnlyList<VariableAssignment> assignments) =>
        new(SolvingStatus.Solvable, assignments);

    public static SatResult Unsolvable() => new(SolvingStatus.Unsolvable, []);

    public static SatResult Indeterminate() => new(SolvingStatus.Indeterminate, []);
}