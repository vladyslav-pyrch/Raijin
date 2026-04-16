using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Raijin.CombinatoricsService.Application.Parsing;

internal static class BoolExprLexer
{
    [StringSyntax(StringSyntaxAttribute.Regex)]
    private static readonly string TruePattern = @"(?<true>\s*true\s*)";

    [StringSyntax(StringSyntaxAttribute.Regex)]
    private static readonly string FalsePattern = @"(?<false>\s*false\s*)";

    [StringSyntax(StringSyntaxAttribute.Regex)]
    private static readonly string VariablePattern = @"(?<var>\s*(?:[a-zA-Z0-9]+|[-_]+[a-zA-Z0-9]+)(?:(?:-+|_+|:+)[a-zA-Z0-9]+)*\s*)";

    [StringSyntax(StringSyntaxAttribute.Regex)]
    private static readonly string LeftBracketPattern = @"(?<l_bracket>\s*\(\s*)";

    [StringSyntax(StringSyntaxAttribute.Regex)]
    private static readonly string RightBracketPattern = @"(?<r_bracket>\s*\)\s*)";

    [StringSyntax(StringSyntaxAttribute.Regex)]
    private static readonly string AndPattern = @"(?<and>\s*\*\s*|\s*&\s*)";

    [StringSyntax(StringSyntaxAttribute.Regex)]
    private static readonly string OrPattern = @"(?<or>\s*\+\s*|\s*\|\s*)";

    [StringSyntax(StringSyntaxAttribute.Regex)]
    private static readonly string NotPattern = @"(?<not>\s*!\s*|\s*~\s*)";

    [StringSyntax(StringSyntaxAttribute.Regex)]
    private static readonly string ImplicationPattern = @"(?<imply>\s*->\s*)";

    [StringSyntax(StringSyntaxAttribute.Regex)]
    private static readonly string ImplicationBackwardPattern = @"(?<imply_backward>\s*<-\s*)";

    [StringSyntax(StringSyntaxAttribute.Regex)]
    private static readonly string EquivalencePattern = @"(?<equal>\s*<->\s*|\s*=\s*)";

    [StringSyntax(StringSyntaxAttribute.Regex)]
    private static readonly string XorPattern = @"(?<xor>\s*\^\s*)";

    [StringSyntax(StringSyntaxAttribute.Regex)]
    private static readonly string UnknownPattern = @"(?<unknown>\s*.+\s*)";

    private static readonly Regex ExpressionRegex = new(string.Join('|', TruePattern, FalsePattern, VariablePattern,
        LeftBracketPattern,
        RightBracketPattern, AndPattern, OrPattern, NotPattern, ImplicationPattern, XorPattern,
        EquivalencePattern, ImplicationBackwardPattern, UnknownPattern));

    public static IEnumerable<Token> Tokenize(string booleanExpression)
    {
        foreach (Match match in ExpressionRegex.Matches(booleanExpression))
            if (match.Groups["true"].Success)
                yield return new Token(match.Value.Trim(), match.Index, TokenType.True);
            else if (match.Groups["false"].Success)
                yield return new Token(match.Value.Trim(), match.Index, TokenType.False);
            else if (match.Groups["var"].Success)
                yield return new Token(match.Value.Trim(), match.Index, TokenType.Variable);
            else if (match.Groups["l_bracket"].Success)
                yield return new Token(match.Value.Trim(), match.Index, TokenType.LeftBracket);
            else if (match.Groups["r_bracket"].Success)
                yield return new Token(match.Value.Trim(), match.Index, TokenType.RightBracket);
            else if (match.Groups["and"].Success)
                yield return new Token(match.Value.Trim(), match.Index, TokenType.And);
            else if (match.Groups["or"].Success)
                yield return new Token(match.Value.Trim(), match.Index, TokenType.Or);
            else if (match.Groups["not"].Success)
                yield return new Token(match.Value.Trim(), match.Index, TokenType.Not);
            else if (match.Groups["equal"].Success)
                yield return new Token(match.Value.Trim(), match.Index, TokenType.Equivalence);
            else if (match.Groups["xor"].Success)
                yield return new Token(match.Value.Trim(), match.Index, TokenType.Xor);
            else if (match.Groups["imply"].Success)
                yield return new Token(match.Value.Trim(), match.Index, TokenType.Implication);
            else if (match.Groups["imply_backward"].Success)
                yield return new Token(match.Value.Trim(), match.Index, TokenType.ImplicationBackward);
            else if (match.Groups["unknown"].Success)
                yield return new Token(match.Value.Trim(), match.Index, TokenType.Unknown);
    }
}
