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
    public override int Precedence => 60;

    public override IEnumerable<BoolVar> GetVariables() => [this];

    public override string ToString() => Name;

    [GeneratedRegex(VariableNamePatterns.VariableNameFull)]
    private static partial Regex VariableNameRegex();
}