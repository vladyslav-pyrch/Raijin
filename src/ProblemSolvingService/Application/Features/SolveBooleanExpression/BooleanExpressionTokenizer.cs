using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Raijin.ProblemSolvingService.Application.Features.SolveBooleanExpression;

public static class BooleanExpressionTokenizer
{
    [StringSyntax(StringSyntaxAttribute.Regex)]
    private static readonly string VariablePattern = @"(?<var>\s*[a-zA-Z_]*[a-zA-Z]+[a-zA-Z_\d]*\s*)";

    [StringSyntax(StringSyntaxAttribute.Regex)]
    private static readonly string LeftBracketPattern = @"(?<l_bracket>\s*\(\s*)";

    [StringSyntax(StringSyntaxAttribute.Regex)]
    private static readonly string RightBracketPattern = @"(?<r_bracket>\s*\)\s*)" ;

    [StringSyntax(StringSyntaxAttribute.Regex)]
    private static readonly string AndPattern = @"(?<and>\s*\*\s*|\s*&\s*)";

    [StringSyntax(StringSyntaxAttribute.Regex)]
    private static readonly string NandPattern = @"(?<nand>\s*~\s*\*\s*|\s*~\s*&\s*)";

    [StringSyntax(StringSyntaxAttribute.Regex)]
    private static readonly string OrPattern = @"(?<or>\s*\+\s*|\s*\|\s*)";

    [StringSyntax(StringSyntaxAttribute.Regex)]
    private static readonly string NorPattern = @"(?<nor>\s*~\s*\+\s*|\s*~\s*\|\s*)";

    [StringSyntax(StringSyntaxAttribute.Regex)]
    private static readonly string NotPattern = @"(?<not>\s*~\s*)";

    [StringSyntax(StringSyntaxAttribute.Regex)]
    private static readonly string ImplicationPattern = @"(?<imply>\s*=>\s*)";

    [StringSyntax(StringSyntaxAttribute.Regex)]
    private static readonly string ImplicationBackwardPattern = @"(?<imply_backward>\s*<=\s*)";

    [StringSyntax(StringSyntaxAttribute.Regex)]
    private static readonly string EquivalencePattern = @"(?<equal>\s*<=>\s*|\s*=\s*)";

    [StringSyntax(StringSyntaxAttribute.Regex)]
    private static readonly string XorPattern = @"(?<xor>\s*\^\s*)";

    [StringSyntax(StringSyntaxAttribute.Regex)]
    private static readonly string UnknownPattern = @"(?<unknown>\s*.+\s*)";

    private static readonly Regex ExpressionRegex = new(string.Join('|', VariablePattern, LeftBracketPattern,
        RightBracketPattern, NandPattern, AndPattern, NorPattern, OrPattern, NotPattern,ImplicationPattern, XorPattern,
        EquivalencePattern, ImplicationBackwardPattern, UnknownPattern));

    public static IEnumerable<BooleanToken> Tokenize(string booleanExpression)
    {
        foreach (Match match in ExpressionRegex.Matches(booleanExpression))
        {
            if (match.Groups["var"].Success)
                yield return new BooleanToken(match.Value.Trim(), match.Index, BooleanTokenType.Variable);
            else if (match.Groups["l_bracket"].Success)
                yield return new BooleanToken(match.Value.Trim(), match.Index, BooleanTokenType.LeftBracket);
            else if (match.Groups["r_bracket"].Success)
                yield return new BooleanToken(match.Value.Trim(), match.Index, BooleanTokenType.RightBracket);
            else if (match.Groups["and"].Success)
                yield return new BooleanToken(match.Value.Trim(), match.Index, BooleanTokenType.And);
            else if (match.Groups["nand"].Success)
                yield return new BooleanToken(match.Value.Trim(), match.Index, BooleanTokenType.Nand);
            else if (match.Groups["or"].Success)
                yield return new BooleanToken(match.Value.Trim(), match.Index, BooleanTokenType.Or);
            else if (match.Groups["nor"].Success)
                yield return new BooleanToken(match.Value.Trim(), match.Index, BooleanTokenType.Nor);
            else if (match.Groups["not"].Success)
                yield return new BooleanToken(match.Value.Trim(), match.Index, BooleanTokenType.Not);
            else if (match.Groups["equal"].Success)
                yield return new BooleanToken(match.Value.Trim(), match.Index, BooleanTokenType.Equivalence);
            else if (match.Groups["xor"].Success)
                yield return new BooleanToken(match.Value.Trim(), match.Index, BooleanTokenType.Xor);
            else if (match.Groups["imply"].Success)
                yield return new BooleanToken(match.Value.Trim(), match.Index, BooleanTokenType.Implication);
            else if (match.Groups["imply_backward"].Success)
                yield return new BooleanToken(match.Value.Trim(), match.Index, BooleanTokenType.ImplicationBackward);
            else if (match.Groups["unknown"].Success)
                yield return new BooleanToken(match.Value.Trim(), match.Index, BooleanTokenType.Unknown);
        }
    }
}