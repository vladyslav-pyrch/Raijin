namespace Raijin.CombinatoricsService.Domain.Problems;

public sealed class Problem
{
    private Problem(Guid id, string name, string description, string problemType)
    {
        Id = id;
        Name = name;
        Description = description;
        ProblemType = problemType;
    }

    public Guid Id { get; }

    public string Name { get; private set; }

    public string Description { get; private set; }

    public string ProblemType { get; }

    public Instance? Instance { get; private set; }

    public SatEncoding? SatEncoding { get; private set; }

    public SatRun? SatRun { get; private set; }

    public Solution? Solution { get; private set; }

    public static Problem Create(Guid id, string name, string description, string problemType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(description);
        ArgumentException.ThrowIfNullOrWhiteSpace(problemType);

        if (name.Length > 100)
            throw new ArgumentException("Name cannot exceed 100 characters", nameof(name));
        if (description.Length > 5000)
            throw new ArgumentException("Description cannot exceed 1000 characters", nameof(description));
        if (problemType.Length > 100)
            throw new ArgumentException("Problem kind cannot exceed 100 characters", nameof(problemType));

        return new Problem(id, name, description, problemType);
    }

    public static Problem Rehydrate(Guid id,
        string name,
        string description,
        string problemType,
        Instance? instance,
        SatEncoding? satEncoding,
        SatRun? satRun,
        Solution? solution
    ) => new(id, name, description, problemType)
    {
        Instance = instance,
        SatEncoding = satEncoding,
        SatRun = satRun,
        Solution = solution
    };

    public void UpdateName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (name.Length > 100)
            throw new ArgumentException("Name cannot exceed 100 characters", nameof(name));

        Name = name;
    }

    public void UpdateDescription(string description)
    {
        ArgumentNullException.ThrowIfNull(description);

        if (description.Length > 5000)
            throw new ArgumentException("Description cannot exceed 1000 characters", nameof(description));

        Description = description;
    }

    public void SetInstance(Instance instance)
    {
        ArgumentNullException.ThrowIfNull(instance);
        if (instance.ProblemType() != ProblemType)
            throw new ArgumentException(
                $"Instance problem kind '{instance.ProblemType}' does not match problem type '{ProblemType}'",
                nameof(instance)
            );

        Instance = instance;
        SatEncoding = null!;
        SatRun = null!;
        Solution = null!;
    }

    public void ReduceToSat()
    {
        if (SatEncoding is not null)
            throw new InvalidOperationException("SAT encoding already exists for this instance");

        Instance instance = GetInstanceOrThrow();

        SatEncoding = instance.ReduceToSat();
    }

    public void StartSatRun()
    {
        if (SatEncoding is null)
            throw new InvalidOperationException("Cannot start a SAT run without SAT encoding");

        if (SatRun is not null)
            throw new InvalidOperationException("A SAT run already exists for this reduction");

        SatRun = SatRun.Create();
    }

    public void MarkSatRunAsRunning()
    {
        SatRun satRun = GetSatRunOrThrow();

        satRun.MarkAsRunning();
    }

    public void CompleteSatRun(Satisfiability satisfiability, IReadOnlyList<int> assignment)
    {
        ArgumentNullException.ThrowIfNull(assignment);

        SatRun satRun = GetSatRunOrThrow();

        satRun.Complete(satisfiability, assignment);
    }

    public void FailSatRun(Guid snapshotId)
    {
        SatRun satRun = GetSatRunOrThrow();

        satRun.Fail();
    }

    public void TimeOutSatRun(Guid snapshotId)
    {
        SatRun satRun = GetSatRunOrThrow();

        satRun.TimeOut();
    }

    public void InterpretSolution()
    {
        Instance instance = GetInstanceOrThrow();
        SatEncoding satEncoding = GetSatEncodingOrThrow();
        SatRun satRun = GetSatRunOrThrow();

        if (satRun.Status != SatRunStatus.Completed)
            throw new InvalidOperationException(
                $"Cannot interpret solution from a SAT run in '{satRun.Status}' status");

        if (satRun.Satisfiability != Satisfiability.Satisfiable)
            throw new InvalidOperationException(
                "Cannot interpret solution from an unsatisfiable or unknown SAT result");

        if (Solution is not null)
            throw new InvalidOperationException("Solution already interpreted for this instance");

        Solution = instance.InterpretSolution(satRun.Assignment, satEncoding.VariableMap);
    }

    private Instance GetInstanceOrThrow()
    {
        if (Instance is null)
            throw new InvalidOperationException("Problem instance is not set");
        return Instance;
    }

    private SatEncoding GetSatEncodingOrThrow()
    {
        if (SatEncoding is null)
            throw new InvalidOperationException("SAT encoding is not set");
        return SatEncoding;
    }

    private SatRun GetSatRunOrThrow()
    {
        if (SatRun is null)
            throw new InvalidOperationException("SAT run is not set");
        return SatRun;
    }

    private Solution GetSolutionOrThrow()
    {
        if (Solution is null)
            throw new InvalidOperationException("Problem solution is not set");
        return Solution;
    }
}