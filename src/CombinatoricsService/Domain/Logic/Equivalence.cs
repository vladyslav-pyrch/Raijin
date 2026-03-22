using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.Logic;

public sealed record Equivalence(ExpressionNode LeftNode, ExpressionNode RightNode) : ExpressionNode
{
    public override string ToString()
    {
        return $"({LeftNode} <-> {RightNode})";
    }

    public override IEnumerable<Variable> GetVariables()
    {
        return [..LeftNode.GetVariables(), ..RightNode.GetVariables()];
    }

    protected internal override int TseitinTransform(List<IEnumerable<int>> clauses,
        BijectiveDictionary<Variable, int> symbolTable,
        Func<int> newLiteralId)
    {
        int left = LeftNode.TseitinTransform(clauses, symbolTable, newLiteralId);
        int right = RightNode.TseitinTransform(clauses, symbolTable, newLiteralId);
        int leftIffRight = newLiteralId();

        clauses.Add([leftIffRight, -left, -right]);
        clauses.Add([leftIffRight, left, right]);
        clauses.Add([-leftIffRight, -left, right]);
        clauses.Add([-leftIffRight, left, -right]);

        return leftIffRight;
    }
}