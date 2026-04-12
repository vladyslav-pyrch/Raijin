namespace Raijin.CombinatoricsService.Domain.Problems;

public sealed class Problem
{
    private Problem(Guid id, string name, string description, DateTime createdAt)
    {
        Id = id;
        Name = name;
        Description = description;
        CreatedAt = createdAt;
    }

    public Guid Id { get; }

    public string Name { get; private set; }

    public string Description { get; private set; }

    public string? Solver { get; private set; }

    public Instance? Instance { get; private set; }

    public Solution? Solution { get; private set; }

    public SatEncoding? SatEncoding { get; private set; }

    public SolvingStatus SolvingStatus { get; private set; } = SolvingStatus.NoSatEncoding;

    public Satisfiability Satisfiability { get; private set; } = Satisfiability.Unknown;

    public IReadOnlyList<int> Assignment { get; private set; } = [];

    public DateTime CreatedAt { get; }

    public DateTime UpdatedAt { get; private set; }

    public DateTime? CompletedAt { get; private set; }

    public static Problem Create(Guid id, string name, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(description);

        if (name.Length > 100)
            throw new ArgumentException("Name cannot exceed 100 characters", nameof(name));
        if (description.Length > 5000)
            throw new ArgumentException("Description cannot exceed 5000 characters", nameof(description));

        DateTime now = DateTime.UtcNow;

        return new Problem(id, name, description, now)
        {
            UpdatedAt = now
        };
    }

    public static Problem Rehydrate(Guid id,
        string name,
        string description,
        DateTime createdAt,
        DateTime updatedAt,
        string? solver,
        Instance? instance,
        SatEncoding? satEncoding,
        SolvingStatus solvingStatus,
        Satisfiability satisfiability,
        IReadOnlyList<int> assignment,
        DateTime? completedAt,
        Solution? solution
    ) => new(id, name, description, createdAt)
    {
        UpdatedAt = updatedAt,
        Solver = solver,
        Instance = instance,
        SatEncoding = satEncoding,
        SolvingStatus = solvingStatus,
        Satisfiability = satisfiability,
        Assignment = assignment,
        CompletedAt = completedAt,
        Solution = solution
    };

    public void UpdateName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (name.Length > 100)
            throw new ArgumentException("Name cannot exceed 100 characters", nameof(name));

        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDescription(string description)
    {
        ArgumentNullException.ThrowIfNull(description);

        if (description.Length > 5000)
            throw new ArgumentException("Description cannot exceed 5000 characters", nameof(description));

        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetSolver(string? solver)
    {
        if (Solver == solver)
            return;

        if (SolvingStatus is SolvingStatus.Running)
            throw new InvalidOperationException("Cannot change solver while solving is in progress.");

        Solver = solver;
        Solution = null;
        Satisfiability = Satisfiability.Unknown;
        Assignment = [];
        SolvingStatus = SatEncoding is not null ? SolvingStatus.Pending : SolvingStatus.NoSatEncoding;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetInstance(Instance instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        if (SolvingStatus is SolvingStatus.Running)
            throw new InvalidOperationException("Cannot change instance while solving is in progress.");

        Instance = instance;
        Solution = null;
        SatEncoding = null;
        SolvingStatus = SolvingStatus.Pending;
        Satisfiability = Satisfiability.Unknown;
        Assignment = [];
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReduceToSat()
    {
        if (SolvingStatus is SolvingStatus.Running)
            throw new InvalidOperationException("Cannot re-encode to SAT while solving is in progress.");

        if (Solver is null)
            throw new InvalidOperationException("Cannot reduce to SAT: no solver has been set.");

        Instance instance = GetInstanceOrThrow();

        SatEncoding = instance.ReduceToSat().SatEncoding;
        SolvingStatus = SolvingStatus.Pending;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsRunning()
    {
        if (Instance is null)
            throw new InvalidOperationException("Cannot mark a problem as running without an instance.");
        
        if (SatEncoding is null)
            throw new InvalidOperationException("Cannot mark a problem as running without a SAT encoding.");

        if (SolvingStatus != SolvingStatus.Pending)
            throw new InvalidOperationException($"Cannot mark a problem as running in '{SolvingStatus}' status.");

        SolvingStatus = SolvingStatus.Running;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete(Satisfiability satisfiability, IReadOnlyList<int> assignment)
    {
        ArgumentNullException.ThrowIfNull(assignment);
        EnsureActiveStatus("complete");

        if (satisfiability == Satisfiability.Satisfiable)
            Solution = Instance!.InterpretSolution(assignment);

        Satisfiability = satisfiability;
        Assignment = assignment;
        SolvingStatus = SolvingStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Fail()
    {
        EnsureActiveStatus("fail");

        SolvingStatus = SolvingStatus.Failed;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void TimeOut()
    {
        EnsureActiveStatus("time out");

        SolvingStatus = SolvingStatus.TimedOut;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    private void EnsureActiveStatus(string action)
    {
        if (SolvingStatus is not SolvingStatus.Pending and not SolvingStatus.Running)
            throw new InvalidOperationException($"Cannot {action} a problem in '{SolvingStatus}' status.");
    }

    private Instance GetInstanceOrThrow()
    {
        if (Instance is null)
            throw new InvalidOperationException("Problem instance is not set.");
        return Instance;
    }
}