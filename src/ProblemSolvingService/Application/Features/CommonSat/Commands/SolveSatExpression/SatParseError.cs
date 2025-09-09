using FluentResults;

namespace Raijin.ProblemSolvingService.Application.Features.CommonSat.Commands.SolveSatExpression;

public sealed class SatParseError(string message, int index) : Error($"Error at {index}; {message}");