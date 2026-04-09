using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.SatRuns;

public sealed class SatRun
{
    private SatRun(Guid id, SatEncoding satEncoding, DateTime createdAt)
    {
        Id = id;
        SatEncoding = satEncoding;
        Status = SatRunStatus.Pending;
        Satisfiability = Satisfiability.Unknown;
        Assignment = [];
        CreatedAt = createdAt;
    }

    public Guid Id { get; }

    public SatEncoding SatEncoding { get; }

    public SatRunStatus Status { get; private set; }

    public Satisfiability Satisfiability { get; private set; }

    public IReadOnlyList<int> Assignment { get; private set; }

    public DateTime CreatedAt { get; }

    public DateTime? CompletedAt { get; private set; }

    public static SatRun Create(Guid id, SatEncoding satEncoding)
    {
        ArgumentNullException.ThrowIfNull(satEncoding);

        return new(id, satEncoding, DateTime.UtcNow);
    }

    public static SatRun Rehydrate(
        Guid id,
        SatEncoding satEncoding,
        SatRunStatus status,
        Satisfiability satisfiability,
        IReadOnlyList<int> assignment,
        DateTime createdAt,
        DateTime? completedAt
    ) => new(id, satEncoding, createdAt)
    {
        Status = status,
        Satisfiability = satisfiability,
        Assignment = assignment,
        CompletedAt = completedAt
    };

    public void MarkAsRunning()
    {
        if (Status != SatRunStatus.Pending)
            throw new InvalidOperationException($"Cannot mark a SAT run as running in '{Status}' status.");

        Status = SatRunStatus.Running;
    }

    public void Complete(Satisfiability satisfiability, IReadOnlyList<int> assignment)
    {
        ArgumentNullException.ThrowIfNull(assignment);
        EnsureActiveStatus("complete");

        Satisfiability = satisfiability;
        Assignment = assignment;
        Status = SatRunStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void Fail()
    {
        EnsureActiveStatus("fail");

        Status = SatRunStatus.Failed;
        CompletedAt = DateTime.UtcNow;
    }

    public void TimeOut()
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
}
