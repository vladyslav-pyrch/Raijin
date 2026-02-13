namespace Raijin.SatSolver.Domain.SatProblems;

public class SatProblem
{
    private int[] _solution = [];

    private SatProblem(Guid id, string dimacs)
    {
        Id = id;
        Dimacs = dimacs;
    }

    public static SatProblem Create(string dimacs) => new(Guid.CreateVersion7(), dimacs);

    public Guid Id { get; }

    public string Dimacs { get; }

    public Satisfiability Satisfiability { get; private set; } = Satisfiability.Unknown;

    public int[] Solution => Satisfiability != Satisfiability.Unknown ? _solution : throw new InvalidOperationException("Solution is not available yet.");

    public void SetSolution(int[] solution)
    {
        _solution = solution;
        Satisfiability = solution.Length == 0 ? Satisfiability.Unsatisfiable : Satisfiability.Satisfiable;
    }


}