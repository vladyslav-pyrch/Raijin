using Raijin.CombinatoricsService.Domain.BooleanExpressions;

namespace Raijin.CombinatoricsService.Application.Parsing;

/// <summary>
/// Parses a Boolean expression string into an immutable <see cref="BoolExpr"/> AST.
/// </summary>
public interface IBoolExprParser : IParser<BoolExpr>;
