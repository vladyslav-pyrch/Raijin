using Raijin.CombinatoricsService.Domain.SatRuns;
using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.Problems;

public sealed class Problem
{
    private Problem(Guid id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }

    public Guid Id { get; }

    public string Name { get; private set; }

    public string Description { get; private set; }

    public Instance? Instance { get; private set; }

    public SatEncoding? SatEncoding { get; private set; }

    public Guid? SatRunId { get; private set; }

    public Solution? Solution { get; private set; }

    public static Problem Create(Guid id, string name, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(description);

        if (name.Length > 100)
            throw new ArgumentException("Name cannot exceed 100 characters", nameof(name));
        if (description.Length > 5000)
            throw new ArgumentException("Description cannot exceed 1000 characters", nameof(description));

        return new Problem(id, name, description);
    }

    public static Problem Rehydrate(Guid id,
        string name,
        string description,
        Instance? instance,
        SatEncoding? satEncoding,
        Guid? satRunId,
        Solution? solution
    ) => new(id, name, description)
    {
        Instance = instance,
        SatEncoding = satEncoding,
        SatRunId = satRunId,
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

        Instance = instance;
        SatEncoding = null;
        SatRunId = null;
        Solution = null;
    }

    public void ReduceToSat()
    {
        if (SatEncoding is not null)
            throw new InvalidOperationException("SAT encoding already exists for this instance");

        Instance instance = GetInstanceOrThrow();

        SatEncoding = instance.ReduceToSat();
    }

    public void LinkSatRun(Guid satRunId)
    {
        if (SatEncoding is null)
            throw new InvalidOperationException("Cannot link a SAT run without SAT encoding");

        if (SatRunId is not null)
            throw new InvalidOperationException("A SAT run is already linked to this problem");

        SatRunId = satRunId;
    }

    public void InterpretSolution(Satisfiability satisfiability, IReadOnlyList<int> assignment)
    {
        ArgumentNullException.ThrowIfNull(assignment);

        Instance instance = GetInstanceOrThrow();
        SatEncoding satEncoding = GetSatEncodingOrThrow();

        if (SatRunId is null)
            throw new InvalidOperationException("No SAT run is linked to this problem");

        if (satisfiability != Satisfiability.Satisfiable)
            throw new InvalidOperationException(
                "Cannot interpret solution from an unsatisfiable or unknown SAT result");

        if (Solution is not null)
            throw new InvalidOperationException("Solution already interpreted for this instance");

        Solution = instance.InterpretSolution(assignment, satEncoding.VariableMap);
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
}