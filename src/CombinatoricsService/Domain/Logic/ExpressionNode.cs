using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.Logic;

public abstract record ExpressionNode
{
    public abstract IEnumerable<LeafNode> GetLeaves();

    public SatProblem TseitinTransform()
    {
        var varId = 1;
        BijectiveDictionary<LeafNode, int> symbolTable = [];
        List<Clause> clauses = [];
        
        int lastVariable = TseitinTransform(clauses, symbolTable, newLiteralId: () => varId++);
        clauses.Add(new Clause(literals: [new Literal(lastVariable)]));

        return new SatProblem(clauses);
    }
    
    protected internal abstract int TseitinTransform(List<Clause> clauses, BijectiveDictionary<LeafNode, int> symbolTable, Func<int> newLiteralId);
}