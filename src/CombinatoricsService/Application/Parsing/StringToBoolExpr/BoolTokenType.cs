namespace Raijin.CombinatoricsService.Application.Parsing.StringToBoolExpr;

internal enum BoolTokenType
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