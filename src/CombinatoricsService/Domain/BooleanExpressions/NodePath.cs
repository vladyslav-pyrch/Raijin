namespace Raijin.CombinatoricsService.Domain.BooleanExpressions;

public sealed record NodePath(IReadOnlyList<ChildSelector> Steps)
{
    public static readonly NodePath Root = new([]);

    public static NodePath Parse(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return Root;

        string[] parts = path.Split('.');
        var steps = new ChildSelector[parts.Length];

        for (var i = 0; i < parts.Length; i++)
        {
            if (!Enum.TryParse(parts[i].Trim(), true, out ChildSelector selector))
                throw new ArgumentException(
                    $"Invalid child selector '{parts[i].Trim()}' at position {i}. " +
                    $"Valid values: {string.Join(", ", Enum.GetNames<ChildSelector>())}.",
                    nameof(path));

            steps[i] = selector;
        }

        return new NodePath(steps);
    }

    public NodePath Append(ChildSelector selector) => new([.. Steps, selector]);

    public override string ToString() => Steps.Count == 0 ? "" : string.Join('.', Steps);
}