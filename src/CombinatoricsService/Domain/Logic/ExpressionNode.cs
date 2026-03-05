using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.Logic;

public abstract record ExpressionNode
{
    public abstract IEnumerable<Variable> GetLeaves();

    public TseitinTransformResult TseitinTransform()
    {
        var varId = 1;
        BijectiveDictionary<Variable, int> symbolTable = [];
        List<Clause> clauses = [];
        
        int lastVariable = TseitinTransform(clauses, symbolTable, newLiteralId: () => varId++);
        clauses.Add(new Clause(literals: [new Literal(lastVariable)]));

        return new TseitinTransformResult(new SatProblem(clauses), symbolTable);
    }
    
    protected internal abstract int TseitinTransform(List<Clause> clauses, BijectiveDictionary<Variable, int> symbolTable, Func<int> newLiteralId);
}