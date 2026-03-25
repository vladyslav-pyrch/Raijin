namespace Raijin.CombinatoricsService.Domain.Shared;

public sealed record SatSolution
{
    public SatSolution(int[] literals)
    {
        ArgumentNullException.ThrowIfNull(literals);

        if (literals.Length == 0)
            return;

        if (literals.Any(l => l == 0))
            throw new ArgumentException("A SAT solution literal must not be zero.", nameof(literals));

        int[] sortedAbsoluteValues = [.. literals.Select(Math.Abs).Order()];

        for (var i = 0; i < sortedAbsoluteValues.Length; i++)
            if (sortedAbsoluteValues[i] != i + 1)
                throw new ArgumentException(
                    $"The SAT solution variables must form a contiguous sequence starting at 1. " +
                    $"Expected variable {i + 1} but found {sortedAbsoluteValues[i]}.",
                    nameof(literals));

        Literals = literals;
    }

    public int[] Literals { get; } = [];

    public int NumberOfVariables => Literals.Length;
}
