using Raijin.ProblemSolvingService.Domain.SatProblems;
using Raijin.ProblemSolvingService.Domain.Shared;

namespace Raijin.ProblemSolvingService.Domain.BooleanFormulas;

public interface IBooleanExpression
{
    public IBooleanExpression Desugar();

    protected internal SatVariable TseitinTransform(SatProblem satProblem, BijectiveDictionary<Variable, SatVariable> symbolTable,
        Func<SatVariable> newSatVariable);
}