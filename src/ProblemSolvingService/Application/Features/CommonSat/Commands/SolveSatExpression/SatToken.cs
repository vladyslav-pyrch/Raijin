namespace Raijin.ProblemSolvingService.Application.Features.CommonSat.Commands.SolveSatExpression;

public sealed record SatToken(string Value, int Index, SatTokenType Type);

public enum SatTokenType
{
    Literal,
    LeftBracket,
    RightBracket,
    Unknown
}