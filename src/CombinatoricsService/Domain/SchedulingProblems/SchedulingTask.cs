namespace Raijin.CombinatoricsService.Domain.SchedulingProblems;

/// <summary>
/// A task in a scheduling problem with a duration and required resources.
/// </summary>
public sealed record SchedulingTask(string Id, int Duration, IReadOnlyList<string> RequiredResources);
