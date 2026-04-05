namespace Raijin.SatSolver.Domain.SatProblems;

public sealed record Clause
{
    public Clause(IEnumerable<Literal> literals)
    {
        Literal[] enumerable = literals as Literal[] ?? literals.ToArray();

        if (enumerable.Length == 0)
            throw new ArgumentException("Clause may not be empty", nameof(literals));

        Literals = enumerable;
    }

    public IEnumerable<Literal> Literals { get; }

    internal int MaxSatVariableId => Literals.Select(literal => literal.SatVariableId).Append(0).Max();

    internal string ToDimacs() => string.Join(" ", Literals.Select(literal => literal.ToDimacs())) + " 0";
}