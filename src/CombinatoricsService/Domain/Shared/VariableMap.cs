namespace Raijin.CombinatoricsService.Domain.Shared;

public sealed record VariableMap
{
    public VariableMap(IReadOnlyDictionary<int, string> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        Entries = entries;
    }

    public IReadOnlyDictionary<int, string> Entries { get; }
}
