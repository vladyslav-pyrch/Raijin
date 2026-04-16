namespace Raijin.CombinatoricsService.Domain.Patterns;

/// <summary>
/// Centralised regex pattern strings for variable name rules used across the domain.
/// </summary>
public static class VariableNamePatterns
{
    /// <summary>
    /// Core sub-expression (no anchors) matching a single variable name token.
    /// Can be embedded inside larger patterns (e.g., lexer alternations).
    /// </summary>
    public const string VariableNameCore =
        @"(?:[a-zA-Z0-9]+|[-_]+[a-zA-Z0-9]+)(?:(?:-+|_+|:+)[a-zA-Z0-9]+)*";

    /// <summary>
    /// Full anchored pattern matching an entire variable name string.
    /// </summary>
    public const string VariableNameFull = $"^{VariableNameCore}$";

    /// <summary>
    /// Full anchored pattern matching a SAT literal: optional leading '~' for negation
    /// followed by a valid variable name.
    /// </summary>
    public const string LiteralFull = $"^~?{VariableNameCore}$";
}
