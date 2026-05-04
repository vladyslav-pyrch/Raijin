using Raijin.CombinatoricsService.Domain.BooleanExpressions;
using Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;

namespace Raijin.CombinatoricsService.Domain.Problems.Boolean;

internal static class TseitinTransform
{
    internal static TseitinTransformResult Apply(BooleanProblemInstance instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        Dictionary<BoolVar, SatVariable> symbolTable = [];
        List<Clause> clauses = [];

        Literal rootLiteral = Transform(instance.Root, clauses, symbolTable);
        clauses.Add(new Clause([rootLiteral]));

        return new TseitinTransformResult(new BooleanSatisfiabilityInstance(clauses), symbolTable);
    }

    private static Literal Transform(
        BoolExpr node,
        List<Clause> clauses,
        Dictionary<BoolVar, SatVariable> symbolTable) =>
        node switch
        {
            BoolVar v => TransformVariable(v, symbolTable),
            ConstExpr c => TransformConst(c, clauses),
            Not n => TransformNot(n, clauses, symbolTable),
            And a => TransformAnd(a, clauses, symbolTable),
            Or o => TransformOr(o, clauses, symbolTable),
            Imply i => TransformImply(i, clauses, symbolTable),
            Xor x => TransformXor(x, clauses, symbolTable),
            Equal e => TransformEqual(e, clauses, symbolTable),
            _ => throw new InvalidOperationException($"Unsupported BoolExpr node type: {node.GetType().Name}")
        };

    private static Literal Pos(SatVariable v) => new(v, false);

    private static Literal Neg(Literal lit) => lit with
    {
        Negated = !lit.Negated
    };

    private static string NewPrefix() => Path.GetRandomFileName().Replace(".", ""); // random prefix for auxiliary variable naming

    private static Literal TransformVariable(BoolVar variable, Dictionary<BoolVar, SatVariable> symbolTable)
    {
        if (symbolTable.TryGetValue(variable, out SatVariable? satVar))
            return Pos(satVar);

        satVar = new SatVariable(variable.Name);
        symbolTable[variable] = satVar;
        return Pos(satVar);
    }

    private static Literal TransformConst(ConstExpr constant, List<Clause> clauses)
    {
        var v = new SatVariable($"{NewPrefix()}-const::{(constant.Value ? 1 : 0)}");
        Literal lit = Pos(v);
        clauses.Add(new Clause([constant.Value ? lit : Neg(lit)]));
        return lit;
    }

    // g ↔ ¬a  →  (g ∨ a) ∧ (¬g ∨ ¬a)
    private static Literal TransformNot(Not node, List<Clause> clauses, Dictionary<BoolVar, SatVariable> symbolTable)
    {
        Literal operand = Transform(node.Node, clauses, symbolTable);
        var g = new SatVariable($"{NewPrefix()}-not::{GetName(node, operand)}");
        Literal gLit = Pos(g);
        clauses.Add(new Clause([gLit, operand]));
        clauses.Add(new Clause([Neg(gLit), Neg(operand)]));
        return gLit;
    }

    // g ↔ (a ∧ b)  →  (¬g ∨ a) ∧ (¬g ∨ b) ∧ (g ∨ ¬a ∨ ¬b)
    private static Literal TransformAnd(And node, List<Clause> clauses, Dictionary<BoolVar, SatVariable> symbolTable)
    {
        Literal left = Transform(node.LeftNode, clauses, symbolTable);
        Literal right = Transform(node.RightNode, clauses, symbolTable);
        var g = new SatVariable($"{NewPrefix()}-and::{GetName(node, left)}::{GetName(node, right)}");
        Literal gLit = Pos(g);
        clauses.Add(new Clause([Neg(gLit), left]));
        clauses.Add(new Clause([Neg(gLit), right]));
        clauses.Add(new Clause([gLit, Neg(left), Neg(right)]));
        return gLit;
    }

    // g ↔ (a ∨ b)  →  (g ∨ ¬a) ∧ (g ∨ ¬b) ∧ (¬g ∨ a ∨ b)
    private static Literal TransformOr(Or node, List<Clause> clauses, Dictionary<BoolVar, SatVariable> symbolTable)
    {
        Literal left = Transform(node.LeftNode, clauses, symbolTable);
        Literal right = Transform(node.RightNode, clauses, symbolTable);
        var g = new SatVariable($"{NewPrefix()}-or::{GetName(node, left)}::{GetName(node, right)}");
        Literal gLit = Pos(g);
        clauses.Add(new Clause([gLit, Neg(left)]));
        clauses.Add(new Clause([gLit, Neg(right)]));
        clauses.Add(new Clause([Neg(gLit), left, right]));
        return gLit;
    }

    // g ↔ (a → b)  →  (g ∨ a) ∧ (g ∨ ¬b) ∧ (¬g ∨ ¬a ∨ b)
    private static Literal TransformImply(Imply node, List<Clause> clauses, Dictionary<BoolVar, SatVariable> symbolTable)
    {
        Literal premise = Transform(node.Premise, clauses, symbolTable);
        Literal conclusion = Transform(node.Conclusion, clauses, symbolTable);
        var g = new SatVariable($"{NewPrefix()}-imply::{GetName(node, premise)}::{GetName(node, conclusion)}");
        Literal gLit = Pos(g);
        clauses.Add(new Clause([gLit, premise]));
        clauses.Add(new Clause([gLit, Neg(conclusion)]));
        clauses.Add(new Clause([Neg(gLit), Neg(premise), conclusion]));
        return gLit;
    }

    // g ↔ (a ⊕ b)  →  (¬g ∨ a ∨ b) ∧ (¬g ∨ ¬a ∨ ¬b) ∧ (g ∨ ¬a ∨ b) ∧ (g ∨ a ∨ ¬b)
    private static Literal TransformXor(Xor node, List<Clause> clauses, Dictionary<BoolVar, SatVariable> symbolTable)
    {
        Literal left = Transform(node.LeftNode, clauses, symbolTable);
        Literal right = Transform(node.RightNode, clauses, symbolTable);
        var g = new SatVariable($"{NewPrefix()}-xor::{GetName(node, left)}::{GetName(node, right)}");
        Literal gLit = Pos(g);
        clauses.Add(new Clause([Neg(gLit), left, right]));
        clauses.Add(new Clause([Neg(gLit), Neg(left), Neg(right)]));
        clauses.Add(new Clause([gLit, Neg(left), right]));
        clauses.Add(new Clause([gLit, left, Neg(right)]));
        return gLit;
    }

    // g ↔ (a ↔ b)  →  (g ∨ ¬a ∨ ¬b) ∧ (g ∨ a ∨ b) ∧ (¬g ∨ ¬a ∨ b) ∧ (¬g ∨ a ∨ ¬b)
    private static Literal TransformEqual(Equal node, List<Clause> clauses, Dictionary<BoolVar, SatVariable> symbolTable)
    {
        Literal left = Transform(node.LeftNode, clauses, symbolTable);
        Literal right = Transform(node.RightNode, clauses, symbolTable);
        var g = new SatVariable($"{NewPrefix()}-equal::{GetName(node, left)}::{GetName(node, right)}");
        Literal gLit = Pos(g);
        clauses.Add(new Clause([gLit, Neg(left), Neg(right)]));
        clauses.Add(new Clause([gLit, left, right]));
        clauses.Add(new Clause([Neg(gLit), Neg(left), right]));
        clauses.Add(new Clause([Neg(gLit), left, Neg(right)]));
        return gLit;
    }

    private static string GetName(BoolExpr node, Literal value) => node switch
    {
        BoolVar => value.Variable.Name,
        _ => value.Variable.Name.Split('-').First()
    };
}