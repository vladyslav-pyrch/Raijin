namespace Raijin.CombinatoricsService.Application.Reductions;

/// <summary>
/// Reduces a problem of type <typeparamref name="TFrom"/> to an equivalent
/// representation of type <typeparamref name="TTo"/>.
/// </summary>
public interface IReduction<in TFrom, out TTo>
{
    /// <summary>
    /// Performs the reduction.
    /// </summary>
    public TTo Reduce(TFrom input);
}
