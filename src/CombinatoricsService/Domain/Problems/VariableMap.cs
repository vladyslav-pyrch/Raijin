namespace Raijin.CombinatoricsService.Domain.Problems;

public sealed record VariableMap
{
    public VariableMap(IReadOnlyDictionary<int, object> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        Entries = entries;
    }

    public IReadOnlyDictionary<int, object> Entries { get; }
}
