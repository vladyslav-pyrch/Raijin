using System.Text.RegularExpressions;
using Raijin.CombinatoricsService.Domain.Patterns;

namespace Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;

public sealed record SatVariable
{
    private static readonly Regex NameOnlyRegex = new(
        VariableNamePatterns.VariableNameFull,
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(100));

    public static readonly Regex LiteralRegex = new(
        VariableNamePatterns.LiteralFull,
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(100));

    public SatVariable(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (!NameOnlyRegex.IsMatch(name))
            throw new ArgumentException($"Variable name '{name}' is not a valid SAT variable name.", nameof(name));

        Name = name;
    }

    public string Name { get; }

    public static bool IsValidLiteralString(string literal) => LiteralRegex.IsMatch(literal);
}