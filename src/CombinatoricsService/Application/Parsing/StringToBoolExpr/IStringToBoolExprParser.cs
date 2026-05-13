using FluentResults;
using Raijin.CombinatoricsService.Domain.BooleanExpressions;

namespace Raijin.CombinatoricsService.Application.Parsing.StringToBoolExpr;

public interface IStringToBoolExprParser
{
    Result<BoolExpr> Parse(string expression);
}