namespace Raijin.CombinatoricsService.Application.Parsing;

internal enum TokenType
{
    True,
    False,
    Variable,
    LeftBracket,
    RightBracket,
    And,
    Or,
    Not,
    Implication,
    ImplicationBackward,
    Equivalence,
    Xor,
    Unknown
}
