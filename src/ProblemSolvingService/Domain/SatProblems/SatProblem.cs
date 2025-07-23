using System.Text;

namespace Raijin.ProblemSolvingService.Domain.SatProblems;

public sealed class SatProblem
{
    private readonly List<Clause> _clauses = [];

    public IReadOnlyList<Clause> Clauses => _clauses;

    public void AddClause(IReadOnlyList<Literal> literals) => _clauses.Add(new Clause(literals));

    public int GetNumberOfVariables() => Clauses.Select(clause => clause.GetNumberOfVariables()).Max();

    public int GetNumberOfClauses() => Clauses.Count;

    public string ToDimacsString()
    {
        var stringBuilder = new StringBuilder();

        return stringBuilder.Append($"p cnf {GetNumberOfVariables()} {GetNumberOfClauses()}\n") // Cant use AppendLine since I need LF, AppendLine may use CRLF depending on the Environment.NewLine
            .AppendJoin('\n', Clauses.Select(clause => clause.ToDimacsString()))
            .ToString();
    }
}