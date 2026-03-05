namespace Raijin.CombinatoricsService.Domain.Shared;

public interface IReadOnlyBijectiveDictionary<TFrom, TTo> : IReadOnlyDictionary<TFrom, TTo>
{
    public IReadOnlyBijectiveDictionary<TTo, TFrom> Inverse { get; }
}