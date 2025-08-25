namespace Raijin.ProblemSolvingService.Domain.BooleanFormulas;

public class BooleanFormula(IBooleanExpression expression)
{
    public IBooleanExpression Expression => expression;
}