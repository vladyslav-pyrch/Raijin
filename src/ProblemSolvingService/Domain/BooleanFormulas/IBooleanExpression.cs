namespace Raijin.ProblemSolvingService.Domain.BooleanFormulas;

public interface IBooleanExpression
{
    public IBooleanExpression Desugar();
}