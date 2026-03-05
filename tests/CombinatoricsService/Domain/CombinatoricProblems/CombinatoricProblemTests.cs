using Raijin.CombinatoricsService.Domain.CombinatoricProblems;
using Raijin.CombinatoricsService.Domain.Logic;

namespace Raijin.CombinatoricsService.Domain.Tests.CombinatoricProblems;

public class CombinatoricProblemTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void Test1()
    {
        var combinatoric = new CombinatoricProblem(Guid.CreateVersion7());

        combinatoric.AddDecisionVariable("v1", ["red", "blue"]);
        combinatoric.AddDecisionVariable("v2", ["red", "blue"]);
        combinatoric.AddDecisionVariable("v3", ["red", "blue"]);
        combinatoric.AddDecisionVariable("v4", ["red", "blue"]);

        combinatoric.AddConstrain(
            "v1 and v2 are connected",
            formula: new StateNode("v1", "red").Xor(new StateNode("v2", "red"))
                .And(new StateNode("v1", "blue").Xor(new StateNode("v2", "blue")))
        );
        combinatoric.AddConstrain(
            "v2 and v3 are connected",
            formula: new StateNode("v2", "red").Xor(new StateNode("v3", "red"))
                .And(new StateNode("v2", "blue").Xor(new StateNode("v3", "blue")))
        );
        combinatoric.AddConstrain(
            "v3 and v4 are connected",
            formula: new StateNode("v3", "red").Xor(new StateNode("v4", "red"))
                .And(new StateNode("v3", "blue").Xor(new StateNode("v4", "blue")))
        );
        combinatoric.AddConstrain(
            "v2 and v4 are connected",
            formula: new StateNode("v2", "red").Xor(new StateNode("v4", "red"))
                .And(new StateNode("v2", "blue").Xor(new StateNode("v4", "blue")))
        );

        testOutputHelper.WriteLine(combinatoric.ToFormula().ToString());

        testOutputHelper.WriteLine(combinatoric.ToFormula().TseitinTransform().Problem.ToDimacs());

        testOutputHelper.WriteLine(
            string.Join(Environment.NewLine, combinatoric.ToFormula().TseitinTransform().SymbolTable
                .Select(pair => $"{pair.Key.ToString()} - {pair.Value}"))
        );
    }
}