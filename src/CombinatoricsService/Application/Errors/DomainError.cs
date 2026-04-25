using FluentResults;

namespace Raijin.CombinatoricsService.Application.Errors;

public sealed class DomainError(string message) : Error(message);
