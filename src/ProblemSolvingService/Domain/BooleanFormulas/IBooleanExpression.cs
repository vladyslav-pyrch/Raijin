using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Domain.BooleanFormulas;

public interface IBooleanExpression
{
    public IBooleanExpression Desugar();

    protected internal SatVariable TseitinTransform(SatProblem satProblem, Dictionary<Variable, SatVariable> symbolTable,
        Func<SatVariable> newSatVariable);
}