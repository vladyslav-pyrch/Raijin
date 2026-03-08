using Raijin.CombinatoricsService.Domain.Logic;

namespace Raijin.CombinatoricsService.Domain.CombinatoricProblems;

public record Constrain
{
    public Constrain(ExpressionNode formula)
    {
        ArgumentNullException.ThrowIfNull(formula);
        
        if (formula.GetVariables().Any(variable => variable is not StateNode))
            throw new ArgumentException($"All leaf nodes in the formula must be of type {nameof(StateNode)}");
        
        Formula = formula;
    }
    
    public ExpressionNode Formula { get; }
    
    public IEnumerable<StateNode> GetStateNodes() =>
        Formula.GetVariables().OfType<StateNode>();
}