namespace Raijin.CombinatoricsService.Domain.Shared;

public interface IBijectiveDictionary<TFrom, TTo> : IDictionary<TFrom, TTo>
{
    public IBijectiveDictionary<TTo, TFrom> Inverse { get; }
}