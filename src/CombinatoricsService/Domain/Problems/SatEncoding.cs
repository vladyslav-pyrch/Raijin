namespace Raijin.CombinatoricsService.Domain.Problems;

public sealed record SatEncoding
{
    private SatEncoding(IEnumerable<IEnumerable<int>> clauses, VariableMap variableMap)
    {
        Clauses = clauses;
        VariableMap = variableMap;
    }

    public IEnumerable<IEnumerable<int>> Clauses { get; }

    public VariableMap VariableMap { get; }

    public static SatEncoding Create(IEnumerable<IEnumerable<int>> clauses, VariableMap variableMap)
    {
        ArgumentNullException.ThrowIfNull(clauses);
        ArgumentNullException.ThrowIfNull(variableMap);

        return new SatEncoding(clauses, variableMap);
    }

    public static SatEncoding Rehydrate(IEnumerable<IEnumerable<int>> clauses, VariableMap variableMap) =>
        new(clauses, variableMap);
}