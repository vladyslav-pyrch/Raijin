using Raijin.ProblemSolvingService.Domain.Abstractions;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Domain.BooleanFormulas;

public sealed record Variable : ValueObject, IBooleanExpression
{
    public Variable(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    public string Name { get; }

    public IBooleanExpression Desugar() => this;

    public SatVariable TseitinTransform(SatProblem satProblem, Dictionary<Variable, SatVariable> symbolTable, Func<SatVariable> newSatVariable)
    {
        if (!symbolTable.TryGetValue(this, out SatVariable? satVariable))
            symbolTable[this] = satVariable = newSatVariable();
        return satVariable;
    }
}