using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Raijin.CombinatoricsService.Domain.Patterns;

namespace Raijin.CombinatoricsService.Application.Parsing.StringToBoolExpr;

internal static class StringToBoolExprLexer
{
    [StringSyntax(StringSyntaxAttribute.Regex)]
    private static readonly string TruePattern = @"(?<true>\s*true\s*)";

    [StringSyntax(StringSyntaxAttribute.Regex)]
    private static readonly string FalsePattern = @"(?<false>\s*false\s*)";

    [StringSyntax(StringSyntaxAttribute.Regex)]
    private static readonly string VariablePattern = $@"(?<var>\s*{VariableNamePatterns.VariableNameCore}\s*)";

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
    private static readonly string ImplicationPattern = @"(?<imply>\s*=>\s*|\s*->\s*)";

    [StringSyntax(StringSyntaxAttribute.Regex)]
    private static readonly string ImplicationBackwardPattern = @"(?<imply_backward>\s*<=\s*|\s*<-\s*)";

    [StringSyntax(StringSyntaxAttribute.Regex)]
    private static readonly string EquivalencePattern = @"(?<equal>\s*<=>\s*|\s*=\s*|\s*<->\s*)";

    [StringSyntax(StringSyntaxAttribute.Regex)]
    private static readonly string XorPattern = @"(?<xor>\s*\^\s*)";

    [StringSyntax(StringSyntaxAttribute.Regex)]
    private static readonly string UnknownPattern = @"(?<unknown>\s*.+\s*)";

    private static readonly Regex ExpressionRegex = new(string.Join('|', TruePattern, FalsePattern, VariablePattern,
        LeftBracketPattern, RightBracketPattern, AndPattern, OrPattern, NotPattern, ImplicationPattern, XorPattern,
        EquivalencePattern, ImplicationBackwardPattern, UnknownPattern));

    public static IEnumerable<BoolToken> Tokenize(string booleanExpression)
    {
        foreach (Match match in ExpressionRegex.Matches(booleanExpression))
            if (match.Groups["true"].Success)
                yield return new BoolToken(match.Value.Trim(), match.Index, BoolTokenType.True);
            else if (match.Groups["false"].Success)
                yield return new BoolToken(match.Value.Trim(), match.Index, BoolTokenType.False);
            else if (match.Groups["var"].Success)
                yield return new BoolToken(match.Value.Trim(), match.Index, BoolTokenType.Variable);
            else if (match.Groups["l_bracket"].Success)
                yield return new BoolToken(match.Value.Trim(), match.Index, BoolTokenType.LeftBracket);
            else if (match.Groups["r_bracket"].Success)
                yield return new BoolToken(match.Value.Trim(), match.Index, BoolTokenType.RightBracket);
            else if (match.Groups["and"].Success)
                yield return new BoolToken(match.Value.Trim(), match.Index, BoolTokenType.And);
            else if (match.Groups["or"].Success)
                yield return new BoolToken(match.Value.Trim(), match.Index, BoolTokenType.Or);
            else if (match.Groups["not"].Success)
                yield return new BoolToken(match.Value.Trim(), match.Index, BoolTokenType.Not);
            else if (match.Groups["equal"].Success)
                yield return new BoolToken(match.Value.Trim(), match.Index, BoolTokenType.Equivalence);
            else if (match.Groups["xor"].Success)
                yield return new BoolToken(match.Value.Trim(), match.Index, BoolTokenType.Xor);
            else if (match.Groups["imply"].Success)
                yield return new BoolToken(match.Value.Trim(), match.Index, BoolTokenType.Implication);
            else if (match.Groups["imply_backward"].Success)
                yield return new BoolToken(match.Value.Trim(), match.Index, BoolTokenType.ImplicationBackward);
            else if (match.Groups["unknown"].Success)
                yield return new BoolToken(match.Value.Trim(), match.Index, BoolTokenType.Unknown);
    }
}