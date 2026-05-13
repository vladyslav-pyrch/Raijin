namespace Raijin.CombinatoricsService.Application.Parsing.StringToBoolExpr;

internal sealed record BoolToken(string Value, int Index, BoolTokenType Type);