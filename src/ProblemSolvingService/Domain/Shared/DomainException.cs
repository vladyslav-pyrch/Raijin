namespace Raijin.ProblemSolvingService.Domain.Shared;

public sealed class DomainException(string message) : Exception(message);