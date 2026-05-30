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
        Dictionary<BoolVar, SatVariable> symbolTable)
    {
        Dictionary<BoolExpr, Literal> results = new(ReferenceEqualityComparer.Instance);
        Stack<TransformFrame> stack = [];

        stack.Push(new TransformFrame(node, false));

        while (stack.Count > 0)
        {
            TransformFrame frame = stack.Pop();

            if (results.ContainsKey(frame.Node))
                continue;

            if (frame.ChildrenTransformed)
            {
                results[frame.Node] = TransformComposite(frame.Node, clauses, results);
                continue;
            }

            switch (frame.Node)
            {
                case BoolVar v:
                    results[frame.Node] = TransformVariable(v, symbolTable);
                    break;
                case ConstExpr c:
                    results[frame.Node] = TransformConst(c, clauses);
                    break;
                case Not n:
                    stack.Push(new TransformFrame(n, true));
                    PushIfMissing(n.Node, stack, results);
                    break;
                case And a:
                    stack.Push(new TransformFrame(a, true));
                    PushIfMissing(a.RightNode, stack, results);
                    PushIfMissing(a.LeftNode, stack, results);
                    break;
                case Or o:
                    stack.Push(new TransformFrame(o, true));
                    PushIfMissing(o.RightNode, stack, results);
                    PushIfMissing(o.LeftNode, stack, results);
                    break;
                case Imply i:
                    stack.Push(new TransformFrame(i, true));
                    PushIfMissing(i.Conclusion, stack, results);
                    PushIfMissing(i.Premise, stack, results);
                    break;
                case Xor x:
                    stack.Push(new TransformFrame(x, true));
                    PushIfMissing(x.RightNode, stack, results);
                    PushIfMissing(x.LeftNode, stack, results);
                    break;
                case Equal e:
                    stack.Push(new TransformFrame(e, true));
                    PushIfMissing(e.RightNode, stack, results);
                    PushIfMissing(e.LeftNode, stack, results);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported BoolExpr node type: {frame.Node.GetType().Name}");
            }
        }

        return results[node];
    }

    private readonly record struct TransformFrame(BoolExpr Node, bool ChildrenTransformed);

    private static void PushIfMissing(
        BoolExpr node,
        Stack<TransformFrame> stack,
        Dictionary<BoolExpr, Literal> results)
    {
        if (!results.ContainsKey(node))
            stack.Push(new TransformFrame(node, false));
    }

    private static Literal TransformComposite(
        BoolExpr node,
        List<Clause> clauses,
        Dictionary<BoolExpr, Literal> results) =>
        node switch
        {
            Not n => TransformNot(n, results[n.Node], clauses),
            And a => TransformAnd(a, results[a.LeftNode], results[a.RightNode], clauses),
            Or o => TransformOr(o, results[o.LeftNode], results[o.RightNode], clauses),
            Imply i => TransformImply(i, results[i.Premise], results[i.Conclusion], clauses),
            Xor x => TransformXor(x, results[x.LeftNode], results[x.RightNode], clauses),
            Equal e => TransformEqual(e, results[e.LeftNode], results[e.RightNode], clauses),
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
    private static Literal TransformNot(Not node, Literal operand, List<Clause> clauses)
    {
        var g = new SatVariable($"{NewPrefix()}-not::{GetName(node, operand)}");
        Literal gLit = Pos(g);
        clauses.Add(new Clause([gLit, operand]));
        clauses.Add(new Clause([Neg(gLit), Neg(operand)]));
        return gLit;
    }

    // g ↔ (a ∧ b)  →  (¬g ∨ a) ∧ (¬g ∨ b) ∧ (g ∨ ¬a ∨ ¬b)
    private static Literal TransformAnd(And node, Literal left, Literal right, List<Clause> clauses)
    {
        var g = new SatVariable($"{NewPrefix()}-and::{GetName(node, left)}::{GetName(node, right)}");
        Literal gLit = Pos(g);
        clauses.Add(new Clause([Neg(gLit), left]));
        clauses.Add(new Clause([Neg(gLit), right]));
        clauses.Add(new Clause([gLit, Neg(left), Neg(right)]));
        return gLit;
    }

    // g ↔ (a ∨ b)  →  (g ∨ ¬a) ∧ (g ∨ ¬b) ∧ (¬g ∨ a ∨ b)
    private static Literal TransformOr(Or node, Literal left, Literal right, List<Clause> clauses)
    {
        var g = new SatVariable($"{NewPrefix()}-or::{GetName(node, left)}::{GetName(node, right)}");
        Literal gLit = Pos(g);
        clauses.Add(new Clause([gLit, Neg(left)]));
        clauses.Add(new Clause([gLit, Neg(right)]));
        clauses.Add(new Clause([Neg(gLit), left, right]));
        return gLit;
    }

    // g ↔ (a → b)  →  (g ∨ a) ∧ (g ∨ ¬b) ∧ (¬g ∨ ¬a ∨ b)
    private static Literal TransformImply(Imply node, Literal premise, Literal conclusion, List<Clause> clauses)
    {
        var g = new SatVariable($"{NewPrefix()}-imply::{GetName(node, premise)}::{GetName(node, conclusion)}");
        Literal gLit = Pos(g);
        clauses.Add(new Clause([gLit, premise]));
        clauses.Add(new Clause([gLit, Neg(conclusion)]));
        clauses.Add(new Clause([Neg(gLit), Neg(premise), conclusion]));
        return gLit;
    }

    // g ↔ (a ⊕ b)  →  (¬g ∨ a ∨ b) ∧ (¬g ∨ ¬a ∨ ¬b) ∧ (g ∨ ¬a ∨ b) ∧ (g ∨ a ∨ ¬b)
    private static Literal TransformXor(Xor node, Literal left, Literal right, List<Clause> clauses)
    {
        var g = new SatVariable($"{NewPrefix()}-xor::{GetName(node, left)}::{GetName(node, right)}");
        Literal gLit = Pos(g);
        clauses.Add(new Clause([Neg(gLit), left, right]));
        clauses.Add(new Clause([Neg(gLit), Neg(left), Neg(right)]));
        clauses.Add(new Clause([gLit, Neg(left), right]));
        clauses.Add(new Clause([gLit, left, Neg(right)]));
        return gLit;
    }

    // g ↔ (a ↔ b)  →  (g ∨ ¬a ∨ ¬b) ∧ (g ∨ a ∨ b) ∧ (¬g ∨ ¬a ∨ b) ∧ (¬g ∨ a ∨ ¬b)
    private static Literal TransformEqual(Equal node, Literal left, Literal right, List<Clause> clauses)
    {
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
