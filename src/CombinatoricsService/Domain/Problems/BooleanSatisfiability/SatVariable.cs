using System.Text.RegularExpressions;

namespace Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;

public sealed record SatVariable
{
    private static readonly Regex NameOnlyRegex = new(
        @"^(?:[a-zA-Z0-9]+|[-_]+[a-zA-Z0-9]+)(?:(?:-+|_+|:+)[a-zA-Z0-9]+)*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        matchTimeout: TimeSpan.FromMilliseconds(100));

    public static readonly Regex LiteralRegex = new(
        @"^-?(?:[a-zA-Z0-9]+|[-_]+[a-zA-Z0-9]+)(?:(?:-+|_+|:+)[a-zA-Z0-9]+)*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        matchTimeout: TimeSpan.FromMilliseconds(100));

    public string Name { get; }

    public SatVariable(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (!NameOnlyRegex.IsMatch(name))
            throw new ArgumentException($"Variable name '{name}' is not a valid SAT variable name.", nameof(name));

        Name = name;
    }

    public static bool IsValidLiteralString(string literal) =>
        LiteralRegex.IsMatch(literal ?? string.Empty);
}
