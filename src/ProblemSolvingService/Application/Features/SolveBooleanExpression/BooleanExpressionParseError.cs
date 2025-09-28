using FluentResults;

namespace Raijin.ProblemSolvingService.Application.Features.SolveBooleanExpression;

public sealed class BooleanExpressionParseError(string message, int index) : Error($"Error at {index}; {message}");