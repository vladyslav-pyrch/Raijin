using Raijin.CombinatoricsService.Domain.Abstractions;

namespace Raijin.CombinatoricsService.Domain.CombinatoricProblems;

public sealed record CombinatoricProblemCreated(Guid Id)
    : DomainEvent(DateTime.UtcNow);