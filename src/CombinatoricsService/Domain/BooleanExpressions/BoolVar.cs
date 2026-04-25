using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Raijin.CombinatoricsService.Domain.Patterns;

namespace Raijin.CombinatoricsService.Domain.BooleanExpressions;

public partial record BoolVar : BoolExpr
{
    public BoolVar(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (!VariableNameRegex().IsMatch(name))
            throw new ArgumentException(
                "Invalid variable name format. " +
                "Names must start with alphanumeric or a dash/underscore run followed by alphanumeric. " +
                "Separator types ('-', '_', ':', '::', ':::') cannot be mixed within a single run. " +
                "Names must end with an alphanumeric character.",
                nameof(name));

        Name = name;
    }

    public string Name { get; }

    [JsonIgnore]
    public override IReadOnlyList<BoolExpr> Children => [];
    
    [JsonIgnore]
    public override int Precedence => 60;

    protected override BoolExpr WithChildren(IReadOnlyList<BoolExpr> children) => this;

    protected override int ResolveChildIndex(ChildSelector selector) =>
        throw new InvalidOperationException($"{nameof(BoolVar)} is a leaf node and has no children.");

    public override IEnumerable<BoolVar> GetVariables() => [this];

    public override string ToString() => Name;

    [GeneratedRegex(VariableNamePatterns.VariableNameFull)]
    private static partial Regex VariableNameRegex();
}