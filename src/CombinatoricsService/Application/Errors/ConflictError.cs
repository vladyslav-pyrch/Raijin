using FluentResults;

namespace Raijin.CombinatoricsService.Application.Errors;

public sealed class ConflictError(string message) : Error(message);