namespace Raijin.CombinatoricsService.Domain.SchedulingProblems;

/// <summary>
/// A resource that can be assigned to tasks. Tasks requiring the same resource cannot overlap.
/// </summary>
public sealed record Resource(string Id);
