namespace Raijin.CombinatoricsService.Domain.Problems.Scheduling;

/// <summary>
///     A resource that can be assigned to tasks. Tasks requiring the same resource cannot overlap.
/// </summary>
public sealed record Resource(string Id);