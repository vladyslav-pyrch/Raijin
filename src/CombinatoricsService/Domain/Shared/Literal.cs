namespace Raijin.CombinatoricsService.Domain.Shared;

public record Literal
{
    public Literal(int number, bool isNegated = false)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(number);

        Number = number;
        IsNegated = isNegated;
    }

    public int Number { get; }
    public bool IsNegated { get; }
}