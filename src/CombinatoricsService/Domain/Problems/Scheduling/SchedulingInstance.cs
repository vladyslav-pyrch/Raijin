using Raijin.CombinatoricsService.Domain.BooleanExpressions;

namespace Raijin.CombinatoricsService.Domain.Problems.Scheduling;

/// <summary>
///     An immutable scheduling problem instance. Tasks must be scheduled on shared
///     resources subject to precedence and additional Boolean constraints.
/// </summary>
public sealed record SchedulingInstance(
    IReadOnlyList<SchedulingTask> Tasks,
    IReadOnlyList<Resource> Resources,
    IReadOnlyList<PrecedenceConstraint> PrecedenceConstraints,
    IReadOnlyList<BoolExpr> AdditionalConstraints)
{
    /// <summary>Creates an empty scheduling instance.</summary>
    public static SchedulingInstance Empty => new([], [], [], []);
    // ── Task editing ──

    /// <summary>Returns a new instance with <paramref name="task" /> appended.</summary>
    public SchedulingInstance WithTask(SchedulingTask task) =>
        this with { Tasks = [.. Tasks, task] };

    /// <summary>Returns a new instance with the task identified by <paramref name="taskId" /> removed.</summary>
    public SchedulingInstance WithoutTask(string taskId) =>
        this with { Tasks = Tasks.Where(t => t.Id != taskId).ToList() };

    /// <summary>Returns a new instance with the task identified by <paramref name="taskId" /> replaced.</summary>
    public SchedulingInstance WithTaskReplaced(string taskId, SchedulingTask replacement)
    {
        var updated = Tasks.Select(t => t.Id == taskId ? replacement : t).ToList();
        return this with { Tasks = updated };
    }

    // ── Resource editing ──

    /// <summary>Returns a new instance with <paramref name="resource" /> appended.</summary>
    public SchedulingInstance WithResource(Resource resource) =>
        this with { Resources = [.. Resources, resource] };

    /// <summary>Returns a new instance with the resource identified by <paramref name="resourceId" /> removed.</summary>
    public SchedulingInstance WithoutResource(string resourceId) =>
        this with { Resources = Resources.Where(r => r.Id != resourceId).ToList() };

    // ── Precedence editing ──

    /// <summary>Returns a new instance with a precedence constraint appended.</summary>
    public SchedulingInstance WithPrecedence(PrecedenceConstraint precedence) =>
        this with { PrecedenceConstraints = [.. PrecedenceConstraints, precedence] };

    /// <summary>Returns a new instance with the matching precedence constraint removed.</summary>
    public SchedulingInstance WithoutPrecedence(string beforeTaskId, string afterTaskId) =>
        this with
        {
            PrecedenceConstraints = PrecedenceConstraints
                .Where(p => !(p.BeforeTaskId == beforeTaskId && p.AfterTaskId == afterTaskId))
                .ToList()
        };

    // ── Additional constraint editing ──

    /// <summary>Returns a new instance with <paramref name="constraint" /> appended.</summary>
    public SchedulingInstance WithConstraint(BoolExpr constraint) =>
        this with { AdditionalConstraints = [.. AdditionalConstraints, constraint] };

    /// <summary>Returns a new instance with the constraint at <paramref name="index" /> removed.</summary>
    public SchedulingInstance WithoutConstraint(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, AdditionalConstraints.Count);

        return this with { AdditionalConstraints = AdditionalConstraints.Where((_, i) => i != index).ToList() };
    }

    /// <summary>Returns a new instance with the constraint at <paramref name="index" /> replaced.</summary>
    public SchedulingInstance WithConstraintAt(int index, BoolExpr constraint)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, AdditionalConstraints.Count);

        List<BoolExpr> updated = [.. AdditionalConstraints];
        updated[index] = constraint;
        return this with { AdditionalConstraints = updated };
    }
}