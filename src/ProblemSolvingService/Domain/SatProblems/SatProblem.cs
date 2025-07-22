namespace Raijin.ProblemSolvingService.Domain.SatProblems;

public sealed class SatProblem
{
    private readonly List<Clause> _clauses = [];

    public IReadOnlyList<Clause> Clauses => _clauses;

    public void AddClause(IReadOnlyList<Literal> literals) => _clauses.Add(new Clause(literals));

    public int GetNumberOfVariables() => Clauses.Select(clause => clause.GetNumberOfVariables()).Max();

    public int GetNumberOfClauses() => Clauses.Count;
}