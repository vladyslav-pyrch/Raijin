using FluentResults;

namespace Raijin.ProblemSolvingService.Application.Features.SolveSatExpression;

public sealed class SatParseError(string message, int index) : Error($"Error at {index}; {message}");