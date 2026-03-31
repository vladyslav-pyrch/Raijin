namespace Raijin.CombinatoricsService.Domain.Problems;

public sealed record SatEncoding
{
    private SatEncoding(string dimacs, VariableMap variableMap)
    {
        Dimacs = dimacs;
        VariableMap = variableMap;
    }

    public string Dimacs { get; }

    public VariableMap VariableMap { get; }

    public static SatEncoding Create(string dimacs, VariableMap variableMap)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dimacs);
        ArgumentNullException.ThrowIfNull(variableMap);

        return new SatEncoding(dimacs, variableMap);
    }

    public static SatEncoding Rehydrate(string dimacs, VariableMap variableMap) => new(dimacs, variableMap);
}