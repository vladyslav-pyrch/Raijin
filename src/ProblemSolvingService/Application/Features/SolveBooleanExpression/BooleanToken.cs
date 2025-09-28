namespace Raijin.ProblemSolvingService.Application.Features.SolveBooleanExpression;

public sealed record BooleanToken(string Value, int Index, BooleanTokenType Type);

public enum BooleanTokenType
{
    Variable,
    LeftBracket,
    RightBracket,
    And,
    Nand,
    Or,
    Nor,
    Not,
    Implication,
    ImplicationBackward,
    Equivalence,
    Xor,
    Unknown
}