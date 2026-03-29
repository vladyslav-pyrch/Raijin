namespace Raijin.CombinatoricsService.Domain.SchedulingProblems;

/// <summary>
/// A precedence constraint stating that task <see cref="BeforeTaskId"/> must finish
/// before task <see cref="AfterTaskId"/> starts.
/// </summary>
public sealed record PrecedenceConstraint(string BeforeTaskId, string AfterTaskId);
