using System.Text;

namespace Raijin.ProblemSolvingService.Domain.SatProblems;

public sealed class SatProblem
{
    private readonly List<Clause> _clauses = [];

    private readonly Dictionary<int, SatVariable> _variables = [];

    public IReadOnlyList<Clause> Clauses => _clauses;

    public IReadOnlyList<SatVariable> Variables => _variables.Values.ToList().AsReadOnly();

    public void AddClause(Clause clause)
    {
        ArgumentNullException.ThrowIfNull(clause);

        foreach (SatVariable satVariable in clause.Literals.Select(literal => literal.SatVariable))
            _variables.TryAdd(satVariable.Id, satVariable);

        _clauses.Add(clause);
    }

    public void AddClause(params IReadOnlyList<Literal> literals)
    {
        AddClause(new Clause(literals));
    }

    public SatVariable GetVariableById(int id)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(id));

        return _variables.TryGetValue(id, out SatVariable? variable)
            ? variable
            : throw new InvalidOperationException("No variable with such Id.");
    }

    public int GetNumberOfVariables() => _variables.Values.Select(variable => variable.Id).Max();

    public int GetNumberOfClauses() => Clauses.Count;

    public string ToDimacsString()
    {
        var stringBuilder = new StringBuilder();

        return stringBuilder.Append($"p cnf {GetNumberOfVariables()} {GetNumberOfClauses()}\n") // Cant use AppendLine since I need LF, AppendLine may use CRLF depending on the Environment.NewLine
            .AppendJoin('\n', Clauses.Select(clause => clause.ToDimacsString()))
            .ToString();
    }
}