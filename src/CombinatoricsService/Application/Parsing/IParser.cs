namespace Raijin.CombinatoricsService.Application.Parsing;

/// <summary>
///     Parses a string representation into a domain object of type <typeparamref name="T" />.
/// </summary>
public interface IParser<out T>
{
    /// <summary>
    ///     Parses the given <paramref name="input" /> string into an instance of <typeparamref name="T" />.
    /// </summary>
    public T Parse(string input);
}