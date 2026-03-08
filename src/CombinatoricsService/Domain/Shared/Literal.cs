namespace Raijin.CombinatoricsService.Domain.Shared;

public sealed record Literal
{
    public Literal(int number, bool isNegated = false)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(number);

        Number = number;
        IsNegated = isNegated;
    }

    public int Number { get; }
    
    public bool IsNegated { get; }
    
    public string ToDimacs() => $"{(IsNegated ? "-" : "")}{Number}";

    public static Literal Negated(int number) => new(number, isNegated: true);

    public static Literal Affirmed(int number) => new(number, isNegated: false);

    public static implicit operator Literal(int number) => number < 0 ? Negated(-number) : Affirmed(number);
}