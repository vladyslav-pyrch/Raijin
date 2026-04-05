using System.Text;

namespace Raijin.SatSolver.Domain.SatProblems;

public sealed class SatProblem
{
    private SatProblem(
        Guid id,
        SolvingStatus solvingStatus,
        IEnumerable<Clause> clauses,
        Solution solution
    )
    {
        Id = id;
        SolvingStatus = solvingStatus;
        Clauses = clauses;
        Solution = solution;
    }

    public Guid Id { get; private set; }

    public SolvingStatus SolvingStatus { get; private set; }

    public IEnumerable<Clause> Clauses { get; }

    public Solution Solution { get; private set; }

    public int NumberOfVariables => Clauses.Select(clause => clause.MaxSatVariableId).Append(0).Max();

    public int NumberOfClauses => Clauses.Count();

    public static SatProblem Create(
        Guid id,
        IEnumerable<IEnumerable<int>> clauses
    )
    {
        IEnumerable<int>[] enumerable = clauses as IEnumerable<int>[] ?? clauses.ToArray();

        if (enumerable.Length == 0)
            throw new ArgumentException("Clauses mat not be empty", nameof(clauses));

        return new SatProblem(
            id,
            SolvingStatus.Pending,
            enumerable.Select(clause => new Clause(clause.Select(literal => new Literal(literal)))),
            Solution.Unknown()
        );
    }

    public static SatProblem Rehydrate(
        Guid id,
        SolvingStatus solvingStatus,
        IEnumerable<IEnumerable<int>> clauses,
        Satisfiability satisfiability,
        IEnumerable<int> solution
    ) => new(
        id,
        solvingStatus,
        clauses.Select(clause => new Clause(clause.Select(literal => new Literal(literal)))),
        new Solution(solution, satisfiability)
    );

    public void MarkAsSolving()
    {
        if (SolvingStatus is not SolvingStatus.Pending)
            throw new InvalidOperationException("Only pending problems may be marked as solving.");

        SolvingStatus = SolvingStatus.Solving;
    }

    public void Fail()
    {
        if (SolvingStatus is not SolvingStatus.Solving)
            throw new InvalidOperationException("Only solving problems may be failed.");

        SolvingStatus = SolvingStatus.Failed;
    }

    public void TimeOut()
    {
        if (SolvingStatus is not SolvingStatus.Solving)
            throw new InvalidOperationException("Only solving problems may be timeouted.");

        SolvingStatus = SolvingStatus.TimeOut;
    }

    public void Solve(IEnumerable<int> assignments)
    {
        if (SolvingStatus is not SolvingStatus.Solving)
            throw new InvalidOperationException("Only solving problems may be solved.");

        int[] enumerable = assignments as int[] ?? assignments.ToArray();

        SolvingStatus = SolvingStatus.Solved;
        Solution = enumerable.Any() ? Solution.Satisfiable(enumerable) : Solution.Unsatisfiable();
    }

    public string ToDimacs()
    {
        var sb = new StringBuilder();

        sb.AppendLine($"p cnf {NumberOfVariables} {NumberOfClauses}");

        foreach (Clause clause in Clauses)
            sb.AppendLine(clause.ToDimacs());

        return sb.ToString();
    }
}