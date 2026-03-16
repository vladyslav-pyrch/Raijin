namespace Raijin.CombinatoricsService.Domain.Logic;

public enum TokenType
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