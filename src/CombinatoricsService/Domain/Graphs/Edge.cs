using System.Text.RegularExpressions;
using Raijin.CombinatoricsService.Domain.Patterns;

namespace Raijin.CombinatoricsService.Domain.Graphs;

public sealed record Edge
{
    private static readonly Regex LabelRegex = new(
        VariableNamePatterns.VariableNameFull,
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(100));

    public Edge(string label, Vertex u, Vertex v)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);

        if (!LabelRegex.IsMatch(label))
            throw new ArgumentException($"Edge label '{label}' is not a valid variable name.", nameof(label));

        Label = label;
        U = u;
        V = v;
    }

    public string Label { get; }

    public Vertex U { get; }

    public Vertex V { get; }
}