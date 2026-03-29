using Raijin.CombinatoricsService.Domain.BooleanExpressions;
using Raijin.CombinatoricsService.Domain.BooleanSatisfiabilityProblems;

namespace Raijin.CombinatoricsService.Application.Reductions;

/// <summary>
///     Reduces a <see cref="BoolExpr" /> tree to an equisatisfiable <see cref="CnfFormula" />
///     via the Tseitin transformation.
///     Each sub-expression is assigned a fresh auxiliary <see cref="SatVariable" />,
///     and clauses are emitted encoding the relationship between a node and its children.
/// </summary>
public sealed class TseitinReduction : IReduction<BoolExpr, TseitinResult>
{
    /// <inheritdoc />
    public TseitinResult Reduce(BoolExpr input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var nextId = 1;
        Dictionary<BoolVar, int> symbolTable = [];
        List<List<int>> rawClauses = [];

        int rootLiteral = Transform(input, rawClauses, symbolTable, () => nextId++);
        rawClauses.Add([rootLiteral]);

        return BuildResult(rawClauses, symbolTable, nextId - 1);
    }

    private static int Transform(
        BoolExpr node,
        List<List<int>> clauses,
        Dictionary<BoolVar, int> symbolTable,
        Func<int> newLiteralId) =>
        node switch
        {
            BoolVar v => TransformVariable(v, symbolTable, newLiteralId),
            ConstExpr c => TransformConst(c, clauses, newLiteralId),
            Not n => TransformNot(n, clauses, symbolTable, newLiteralId),
            And a => TransformAnd(a, clauses, symbolTable, newLiteralId),
            Or o => TransformOr(o, clauses, symbolTable, newLiteralId),
            Imply i => TransformImply(i, clauses, symbolTable, newLiteralId),
            Xor x => TransformXor(x, clauses, symbolTable, newLiteralId),
            Equal e => TransformEqual(e, clauses, symbolTable, newLiteralId),
            _ => throw new InvalidOperationException(
                $"Unsupported BoolExpr node type: {node.GetType().Name}")
        };

    private static int TransformVariable(
        BoolVar variable,
        Dictionary<BoolVar, int> symbolTable,
        Func<int> newLiteralId)
    {
        if (symbolTable.TryGetValue(variable, out int varId))
            return varId;

        varId = newLiteralId();
        symbolTable[variable] = varId;
        return varId;
    }

    private static int TransformConst(
        ConstExpr constant,
        List<List<int>> clauses,
        Func<int> newLiteralId)
    {
        int literal = newLiteralId();
        clauses.Add([constant.Value ? literal : -literal]);
        return literal;
    }

    // n ↔ ¬a → (n ∨ a) ∧ (¬n ∨ ¬a)
    private static int TransformNot(
        Not node,
        List<List<int>> clauses,
        Dictionary<BoolVar, int> symbolTable,
        Func<int> newLiteralId)
    {
        int operand = Transform(node.Node, clauses, symbolTable, newLiteralId);
        int n = newLiteralId();

        clauses.Add([n, operand]);
        clauses.Add([-n, -operand]);

        return n;
    }

    // g ↔ (a ∧ b) → (¬g ∨ a) ∧ (¬g ∨ b) ∧ (g ∨ ¬a ∨ ¬b)
    private static int TransformAnd(
        And node,
        List<List<int>> clauses,
        Dictionary<BoolVar, int> symbolTable,
        Func<int> newLiteralId)
    {
        int left = Transform(node.LeftNode, clauses, symbolTable, newLiteralId);
        int right = Transform(node.RightNode, clauses, symbolTable, newLiteralId);
        int g = newLiteralId();

        clauses.Add([-g, left]);
        clauses.Add([-g, right]);
        clauses.Add([g, -left, -right]);

        return g;
    }

    // g ↔ (a ∨ b) → (g ∨ ¬a) ∧ (g ∨ ¬b) ∧ (¬g ∨ a ∨ b)
    private static int TransformOr(
        Or node,
        List<List<int>> clauses,
        Dictionary<BoolVar, int> symbolTable,
        Func<int> newLiteralId)
    {
        int left = Transform(node.LeftNode, clauses, symbolTable, newLiteralId);
        int right = Transform(node.RightNode, clauses, symbolTable, newLiteralId);
        int g = newLiteralId();

        clauses.Add([g, -left]);
        clauses.Add([g, -right]);
        clauses.Add([-g, left, right]);

        return g;
    }

    // g ↔ (a → b) → (g ∨ a) ∧ (g ∨ ¬b) ∧ (¬g ∨ ¬a ∨ b)
    private static int TransformImply(
        Imply node,
        List<List<int>> clauses,
        Dictionary<BoolVar, int> symbolTable,
        Func<int> newLiteralId)
    {
        int premise = Transform(node.Premise, clauses, symbolTable, newLiteralId);
        int conclusion = Transform(node.Conclusion, clauses, symbolTable, newLiteralId);
        int g = newLiteralId();

        clauses.Add([g, premise]);
        clauses.Add([g, -conclusion]);
        clauses.Add([-g, -premise, conclusion]);

        return g;
    }

    // g ↔ (a ⊕ b) → (¬g ∨ a ∨ b) ∧ (¬g ∨ ¬a ∨ ¬b) ∧ (g ∨ ¬a ∨ b) ∧ (g ∨ a ∨ ¬b)
    private static int TransformXor(
        Xor node,
        List<List<int>> clauses,
        Dictionary<BoolVar, int> symbolTable,
        Func<int> newLiteralId)
    {
        int left = Transform(node.LeftNode, clauses, symbolTable, newLiteralId);
        int right = Transform(node.RightNode, clauses, symbolTable, newLiteralId);
        int g = newLiteralId();

        clauses.Add([-g, left, right]);
        clauses.Add([-g, -left, -right]);
        clauses.Add([g, -left, right]);
        clauses.Add([g, left, -right]);

        return g;
    }

    // g ↔ (a ↔ b) → (g ∨ ¬a ∨ ¬b) ∧ (g ∨ a ∨ b) ∧ (¬g ∨ ¬a ∨ b) ∧ (¬g ∨ a ∨ ¬b)
    private static int TransformEqual(
        Equal node,
        List<List<int>> clauses,
        Dictionary<BoolVar, int> symbolTable,
        Func<int> newLiteralId)
    {
        int left = Transform(node.LeftNode, clauses, symbolTable, newLiteralId);
        int right = Transform(node.RightNode, clauses, symbolTable, newLiteralId);
        int g = newLiteralId();

        clauses.Add([g, -left, -right]);
        clauses.Add([g, left, right]);
        clauses.Add([-g, -left, right]);
        clauses.Add([-g, left, -right]);

        return g;
    }

    private static TseitinResult BuildResult(
        List<List<int>> rawClauses,
        Dictionary<BoolVar, int> symbolTable,
        int totalVariables)
    {
        var satVariables = new SatVariable[totalVariables];
        for (var i = 0; i < totalVariables; i++)
            satVariables[i] = new SatVariable(i + 1);

        IReadOnlyList<Clause> typedClauses = rawClauses
            .Select(raw => new Clause(
                raw.Select(lit => new Literal(
                    satVariables[Math.Abs(lit) - 1],
                    lit < 0
                )).ToList()))
            .ToList();

        IReadOnlyDictionary<BoolVar, SatVariable> variableMap = symbolTable
            .ToDictionary(
                kvp => kvp.Key,
                kvp => satVariables[kvp.Value - 1]);

        return new TseitinResult(new BooleanSatisfiabilityInstance(typedClauses), variableMap);
    }
}