using System.Text.RegularExpressions;
using Raijin.CombinatoricsService.Domain.Patterns;

namespace Raijin.CombinatoricsService.Domain.Graphs;

public sealed record Vertex
{
    private static readonly Regex NameRegex = new(
        VariableNamePatterns.VariableNameFull,
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        matchTimeout: TimeSpan.FromMilliseconds(100));

    public string Id { get; }

    public Vertex(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        if (!NameRegex.IsMatch(id))
            throw new ArgumentException($"Vertex id '{id}' is not a valid variable name.", nameof(id));

        Id = id;
    }
}
