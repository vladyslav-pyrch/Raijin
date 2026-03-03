using Raijin.CombinatoricsService.Domain.Logic;

namespace Raijin.CombinatoricsService.Domain.CombinatoricProblems;

public record Constrain
{
    public Constrain(string name, ExpressionNode formula)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(formula);
        
        formula.GetLeaves().ToList().ForEach(leaf =>
        {
            if (leaf is not StateNode)
                throw new ArgumentException($"All leaf nodes in the formula must be of type {nameof(StateNode)}");
        });
        
        Name = name;
        Formula = formula;
    }
    
    public string Name { get; }
    
    public ExpressionNode Formula { get; }
    
    public IEnumerable<StateNode> GetStateNodes() =>
        Formula.GetLeaves().OfType<StateNode>();
}