namespace Raijin.ProblemSolvingService.Domain.SatProblems;

public sealed record SatResult
{
    private SatResult(SolvingStatus status, IReadOnlyList<SatVariableAssignment> assignments)
    {
        Status = status;
        Assignments = assignments;
    }

    public SolvingStatus Status { get; }

    public IReadOnlyList<SatVariableAssignment> Assignments { get; }

    public static SatResult Solvable(IReadOnlyList<SatVariableAssignment> assignments) =>
        new(SolvingStatus.Solvable, assignments);

    public static SatResult Unsolvable() => new(SolvingStatus.Unsolvable, []);

    public static SatResult Indeterminate() => new(SolvingStatus.Indeterminate, []);
}