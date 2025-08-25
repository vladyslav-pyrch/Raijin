using System.Text;
using Raijin.ProblemSolvingService.Domain.Abstractions;

namespace Raijin.ProblemSolvingService.Domain.SatProblems;

public sealed record Clause : ValueObject
{
    public Clause(IReadOnlyList<Literal> literals)
    {
        ArgumentNullException.ThrowIfNull(literals, nameof(literals));
        if (literals.Any(element => element is null) || !literals.Any())
            throw new ArgumentException("A literal is null or literals are empty", nameof(literals));

        Literals = literals;
    }

    public IReadOnlyList<Literal> Literals { get; }

    internal int GetNumberOfVariables() => Literals.Select(literal => literal.SatVariable.Id).Max();

    internal string ToDimacsString()
    {
        var stringBuilder = new StringBuilder();

        return stringBuilder.AppendJoin(' ', Literals.Select(literal => literal.ToDimacsString()))
            .Append(" 0") // Clause ending.
            .ToString();
    }
}