using System.Text.RegularExpressions;
using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.Logic;

public partial record Variable : ExpressionNode
{
    public Variable(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (!VariableNameRegex().IsMatch(name))
            throw new ArgumentException(
                "Variable name must start with a letter and can only contain letters, digits, underscore and hyphens.",
                nameof(name));
        
        Name = name;
    }
    
    public string Name { get; }
    
    public override IEnumerable<Variable> GetLeaves() => [this];
    
    protected internal override int TseitinTransform(List<Clause> clauses, BijectiveDictionary<Variable, int> symbolTable, Func<int> newLiteralId)
    {
        if (symbolTable.TryGetValue(this, out int varId))
            return varId;
        
        varId = newLiteralId();
        symbolTable.Add(this, varId);

        return varId;
    }

    [GeneratedRegex("^[a-zA-Z][a-zA-Z0-9-_]*$")]
    private static partial Regex VariableNameRegex();
}