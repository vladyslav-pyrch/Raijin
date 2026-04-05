namespace Raijin.SatSolver.Domain.SatProblems;

public sealed record Literal
{
    public Literal(int value)
    {
        ArgumentOutOfRangeException.ThrowIfZero(value);

        Value = value;
    }

    public int Value { get; }

    public int SatVariableId => Math.Abs(Value);

    public bool IsNegated => Value < 0;

    internal string ToDimacs() => Value.ToString();
}