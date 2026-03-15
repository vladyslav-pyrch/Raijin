namespace Raijin.CombinatoricsService.Domain.CombinatoricProblems;

public record Constraint
{
    public Constraint(ExpressionNode formula)
    {
        ArgumentNullException.ThrowIfNull(formula);
        
        if (formula.Leaves().Any(variable => variable is not StateNode))
            throw new ArgumentException($"All leaf nodes in the formula must be of type {nameof(StateNode)}");
        
        Formula = formula;
    }
    
    public ExpressionNode Formula { get; }
    
    public IEnumerable<StateNode> GetStateNodes() =>
        Formula.Leaves().OfType<StateNode>();
}