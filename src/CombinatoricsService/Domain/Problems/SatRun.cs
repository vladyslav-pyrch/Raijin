namespace Raijin.CombinatoricsService.Domain.Problems;

public sealed class SatRun
{
    private SatRun(DateTime createdAt)
    {
        Status = SatRunStatus.Pending;
        Satisfiability = Satisfiability.Unknown;
        Assignment = [];
        CreatedAt = createdAt;
    }

    public SatRunStatus Status { get; private set; }

    public Satisfiability Satisfiability { get; private set; }

    public IReadOnlyList<int> Assignment { get; private set; }

    public DateTime CreatedAt { get; }

    public DateTime? CompletedAt { get; private set; }

    internal static SatRun Create() => new(DateTime.UtcNow);

    internal void MarkAsRunning()
    {
        if (Status != SatRunStatus.Pending)
            throw new InvalidOperationException($"Cannot mark a SAT run as running in '{Status}' status.");

        Status = SatRunStatus.Running;
    }

    internal void Complete(Satisfiability satisfiability, IReadOnlyList<int> assignment)
    {
        ArgumentNullException.ThrowIfNull(assignment);
        EnsureActiveStatus("complete");

        Satisfiability = satisfiability;
        Assignment = assignment;
        Status = SatRunStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    internal void Fail()
    {
        EnsureActiveStatus("fail");

        Status = SatRunStatus.Failed;
        CompletedAt = DateTime.UtcNow;
    }

    internal void TimeOut()
    {
        EnsureActiveStatus("time out");

        Status = SatRunStatus.TimedOut;
        CompletedAt = DateTime.UtcNow;
    }

    private void EnsureActiveStatus(string action)
    {
        if (Status is not SatRunStatus.Pending and not SatRunStatus.Running)
            throw new InvalidOperationException($"Cannot {action} a SAT run in '{Status}' status.");
    }

    public static SatRun Rehydrate(
        Guid id,
        SatRunStatus status,
        Satisfiability satisfiability,
        IReadOnlyList<int> assignment,
        DateTime createdAt,
        DateTime? completedAt
    ) => new(createdAt)
    {
        Status = status,
        Satisfiability = satisfiability,
        Assignment = assignment,
        CompletedAt = completedAt
    };
}