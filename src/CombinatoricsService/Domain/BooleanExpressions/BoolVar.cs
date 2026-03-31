using System.Text.RegularExpressions;

namespace Raijin.CombinatoricsService.Domain.BooleanExpressions;

public partial record BoolVar : BoolExpr
{
    public BoolVar(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (!VariableNameRegex().IsMatch(name))
            throw new ArgumentException(
                "Variable name must start with a letter and can only contain letters, digits, underscore and hyphens.",
                nameof(name));

        Name = name;
    }

    public string Name { get; }

    public override IReadOnlyList<BoolExpr> Children => [];

    protected override BoolExpr WithChildren(IReadOnlyList<BoolExpr> children) => this;

    protected override int ResolveChildIndex(ChildSelector selector) =>
        throw new InvalidOperationException($"{nameof(BoolVar)} is a leaf node and has no children.");

    public override IEnumerable<BoolVar> GetVariables() => [this];

    public override string ToString() => Name;

    [GeneratedRegex("^[a-zA-Z][a-zA-Z0-9-_]*$")]
    private static partial Regex VariableNameRegex();
}