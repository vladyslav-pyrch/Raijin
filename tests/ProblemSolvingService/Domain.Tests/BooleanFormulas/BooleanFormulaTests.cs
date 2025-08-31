using FluentAssertions;
using NSubstitute;
using Raijin.ProblemSolvingService.Domain.BooleanFormulas;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Domain.Tests.BooleanFormulas;

[Trait("Category", "Unit")]
public class BooleanFormulaTests
{
    [Fact]
    public void GivenVariable_WhenTransforming_ThenReturnsAppropriateSatProblemAndSymbolTable()
    {
        var variable = new Variable("1");
        var satProblem = new SatProblem();

        Dictionary<Variable, SatVariable> symbolTable = new BooleanFormula(variable).TransformToSat(satProblem);

        satProblem.Clauses.Should().BeEmpty();
        symbolTable.Should().BeEquivalentTo(new Dictionary<Variable, SatVariable>
        {
            [new Variable("1")] = new(1)
        });
    }

    [Fact]
    public void GivenNegation_WhenTransforming_ThenReturnsAppropriateSatProblemAndSymbolTable()
    {
        Negation negation = new Variable("1").Negated();
        var satProblem = new SatProblem();

        Dictionary<Variable, SatVariable> symbolTable = new BooleanFormula(negation).TransformToSat(satProblem);

        satProblem.Clauses.Should().BeEquivalentTo([
            new Clause(Literal.FromInteger(1), Literal.FromInteger(2)),
            new Clause(Literal.FromInteger(-1), Literal.FromInteger(-2))
        ]);
        symbolTable.Should().BeEquivalentTo(new Dictionary<Variable, SatVariable>
        {
            [new Variable("1")] = new(1)
        });
    }

    [Fact]
    public void GivenConjunction_WhenTransforming_ThenReturnsAppropriateSatProblemAndSymbolTable()
    {
        Conjunction conjunction = new Variable("1").And(new Variable("2"));
        var satProblem = new SatProblem();

        Dictionary<Variable, SatVariable> symbolTable = new BooleanFormula(conjunction).TransformToSat(satProblem);

        satProblem.Clauses.Should().BeEquivalentTo([
            new Clause(Literal.FromInteger(1), Literal.FromInteger(-3)),
            new Clause(Literal.FromInteger(2), Literal.FromInteger(-3)),
            new Clause(Literal.FromInteger(-1), Literal.FromInteger(-2), Literal.FromInteger(3))
        ]);
        symbolTable.Should().BeEquivalentTo(new Dictionary<Variable, SatVariable>
        {
            [new Variable("1")] = new(1),
            [new Variable("2")] = new(2)
        });
    }

    [Fact]
    public void GivenNegatedConjunction_WhenTransforming_ThenReturnsAppropriateSatProblemAndSymbolTable()
    {
        NegatedConjunction conjunction = new Variable("1").Nand(new Variable("2"));
        var satProblem = new SatProblem();

        Dictionary<Variable, SatVariable> symbolTable = new BooleanFormula(conjunction).TransformToSat(satProblem);

        satProblem.Clauses.Should().BeEquivalentTo([
            new Clause(Literal.FromInteger(1), Literal.FromInteger(3)),
            new Clause(Literal.FromInteger(2), Literal.FromInteger(3)),
            new Clause(Literal.FromInteger(-1), Literal.FromInteger(-2), Literal.FromInteger(-3))
        ]);
        symbolTable.Should().BeEquivalentTo(new Dictionary<Variable, SatVariable>
        {
            [new Variable("1")] = new(1),
            [new Variable("2")] = new(2)
        });
    }

    [Fact]
    public void GivenDisjunction_WhenTransforming_ThenReturnsAppropriateSatProblemAndSymbolTable()
    {
        Disjunction disjunction = new Variable("1").Or(new Variable("2"));
        var satProblem = new SatProblem();

        Dictionary<Variable, SatVariable> symbolTable = new BooleanFormula(disjunction).TransformToSat(satProblem);

        satProblem.Clauses.Should().BeEquivalentTo([
            new Clause(Literal.FromInteger(-1), Literal.FromInteger(3)),
            new Clause(Literal.FromInteger(-2), Literal.FromInteger(3)),
            new Clause(Literal.FromInteger(1), Literal.FromInteger(2), Literal.FromInteger(-3))
        ]);
        symbolTable.Should().BeEquivalentTo(new Dictionary<Variable, SatVariable>
        {
            [new Variable("1")] = new(1),
            [new Variable("2")] = new(2)
        });
    }

    [Fact]
    public void GivenNegatedDisjunction_WhenTransforming_ThenReturnsAppropriateSatProblemAndSymbolTable()
    {
        NegatedDisjunction disjunction = new Variable("1").Nor(new Variable("2"));
        var satProblem = new SatProblem();

        Dictionary<Variable, SatVariable> symbolTable = new BooleanFormula(disjunction).TransformToSat(satProblem);

        satProblem.Clauses.Should().BeEquivalentTo([
            new Clause(Literal.FromInteger(-1), Literal.FromInteger(-3)),
            new Clause(Literal.FromInteger(-2), Literal.FromInteger(-3)),
            new Clause(Literal.FromInteger(1), Literal.FromInteger(2), Literal.FromInteger(3))
        ]);
        symbolTable.Should().BeEquivalentTo(new Dictionary<Variable, SatVariable>
        {
            [new Variable("1")] = new(1),
            [new Variable("2")] = new(2)
        });
    }

    [Fact]
    public void GivenEquivalence_WhenTransforming_ThenReturnsAppropriateSatProblemAndSymbolTable()
    {
        Equivalence equivalence = new Variable("1").Equal(new Variable("2"));
        var satProblem = new SatProblem();

        Dictionary<Variable, SatVariable> symbolTable = new BooleanFormula(equivalence).TransformToSat(satProblem);

        satProblem.Clauses.Should().BeEquivalentTo([
            new Clause(Literal.FromInteger(-1), Literal.FromInteger(-2), Literal.FromInteger(3)),
            new Clause(Literal.FromInteger(1), Literal.FromInteger(2), Literal.FromInteger(3)),
            new Clause(Literal.FromInteger(1), Literal.FromInteger(-2), Literal.FromInteger(-3)),
            new Clause(Literal.FromInteger(-1), Literal.FromInteger(2), Literal.FromInteger(-3))
        ]);
        symbolTable.Should().BeEquivalentTo(new Dictionary<Variable, SatVariable>
        {
            [new Variable("1")] = new(1),
            [new Variable("2")] = new(2)
        });
    }

    [Fact]
    public void GivenExclusiveDisjunction_WhenTransforming_ThenReturnsAppropriateSatProblemAndSymbolTable()
    {
        ExclusiveDisjunction disjunction = new Variable("1").Xor(new Variable("2"));
        var satProblem = new SatProblem();

        Dictionary<Variable, SatVariable> symbolTable = new BooleanFormula(disjunction).TransformToSat(satProblem);

        satProblem.Clauses.Should().BeEquivalentTo([
            new Clause(Literal.FromInteger(-1), Literal.FromInteger(-2), Literal.FromInteger(-3)),
            new Clause(Literal.FromInteger(1), Literal.FromInteger(2), Literal.FromInteger(-3)),
            new Clause(Literal.FromInteger(1), Literal.FromInteger(-2), Literal.FromInteger(3)),
            new Clause(Literal.FromInteger(-1), Literal.FromInteger(2), Literal.FromInteger(3)),
        ]);
        symbolTable.Should().BeEquivalentTo(new Dictionary<Variable, SatVariable>
        {
            [new Variable("1")] = new(1),
            [new Variable("2")] = new(2)
        });
    }

    [Fact]
    public void GivenImplication_WhenTransforming_ThenReturnsAppropriateSatProblemAndSymbolTable()
    {
        Implication disjunction = new Variable("1").Imply(new Variable("2"));
        var satProblem = new SatProblem();

        Dictionary<Variable, SatVariable> symbolTable = new BooleanFormula(disjunction).TransformToSat(satProblem);

        satProblem.Clauses.Should().BeEquivalentTo([
            new Clause(Literal.FromInteger(1), Literal.FromInteger(3)),
            new Clause(Literal.FromInteger(-2), Literal.FromInteger(3)),
            new Clause(Literal.FromInteger(-1), Literal.FromInteger(2), Literal.FromInteger(-3))
        ]);
        symbolTable.Should().BeEquivalentTo(new Dictionary<Variable, SatVariable>
        {
            [new Variable("1")] = new(1),
            [new Variable("2")] = new(2)
        });
    }
}