namespace Raijin.CombinatoricsService.Domain.Shared;

public static class Extensions
{
    public static IDictionary<TL, TR> Invert<TR, TL>(this IDictionary<TR, TL> dictionary)
        where TL : notnull
        where TR : notnull
        => dictionary.ToDictionary(kv => kv.Value, kv => kv.Key);
    
    public static IReadOnlyDictionary<TL, TR> Invert<TR, TL>(this IReadOnlyDictionary<TR, TL> dictionary)
        where TL : notnull
        where TR : notnull
        => dictionary.ToDictionary(kv => kv.Value, kv => kv.Key);
    
    public static Dictionary<TL, TR> Invert<TR, TL>(this Dictionary<TR, TL> dictionary)
        where TL : notnull
        where TR : notnull
        => dictionary.ToDictionary(kv => kv.Value, kv => kv.Key);
}