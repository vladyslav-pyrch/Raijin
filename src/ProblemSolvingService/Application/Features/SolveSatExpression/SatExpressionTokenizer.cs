using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Raijin.ProblemSolvingService.Application.Features.SolveSatExpression;

public static class SatExpressionTokenizer
{
    [StringSyntax(StringSyntaxAttribute.Regex)]
    private static readonly string LiteralPattern = @"(?<literal>\s*~?[A-Za-z\d]+\s*)";

    [StringSyntax(StringSyntaxAttribute.Regex)]
    private static readonly string LeftBracketPattern = @"(?<l_bracket>\s*\(\s*)";

    [StringSyntax(StringSyntaxAttribute.Regex)]
    private static readonly string RightPatternPattern = @"(?<r_bracket>\s*\)\s*)";

    [StringSyntax(StringSyntaxAttribute.Regex)]
    private static readonly string UnknownPattern = @"(?<unknow>[^\s]+\s*)";

    private static readonly Regex ExpressionRegex =
        new(string.Join('|', LiteralPattern, LeftBracketPattern, RightPatternPattern, UnknownPattern));

    public static IEnumerable<SatToken> Tokenize(string satExpression)
    {
        foreach (Match match in ExpressionRegex.Matches(satExpression))
        {
            if (match.Groups["l_bracket"].Success)
                yield return new SatToken("(", match.Index, SatTokenType.LeftBracket);
            else if (match.Groups["r_bracket"].Success)
                yield return new SatToken(")", match.Index, SatTokenType.RightBracket);
            else if (match.Groups["literal"].Success)
                yield return new SatToken(match.Value.Trim(), match.Index, SatTokenType.Literal);
            else if (match.Groups["unknow"].Success)
                yield return new SatToken(match.Value.Trim(), match.Index, SatTokenType.Unknown);
        }
    }
}