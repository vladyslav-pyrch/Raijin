namespace Raijin.CombinatoricsService.Domain.Logic;

public class BooleanProblem
{
    public BooleanProblem(Guid id, string formula)
    {
        Id = id;
        Formula = formula;
        Expression = ExpressionParser.Parse(formula);
    }
    
    public Guid Id { get; }
    
    public string Formula { get; }
    
    public ExpressionNode Expression { get; }
}