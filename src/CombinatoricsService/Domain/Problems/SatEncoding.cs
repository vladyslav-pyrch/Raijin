namespace Raijin.CombinatoricsService.Domain.Problems;

public sealed record SatEncoding
{
    public SatEncoding(string dimacs, VariableMap variableMap)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dimacs);
        ArgumentNullException.ThrowIfNull(variableMap);
        Dimacs = dimacs;
        VariableMap = variableMap;
    }

    public string Dimacs { get; }

    public VariableMap VariableMap { get; }
}