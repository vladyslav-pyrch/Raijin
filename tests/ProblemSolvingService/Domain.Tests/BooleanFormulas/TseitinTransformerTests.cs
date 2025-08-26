using System.Text.Json;
using FluentAssertions;
using NSubstitute;
using Raijin.ProblemSolvingService.Domain.BooleanFormulas;
using Raijin.ProblemSolvingService.Domain.SatProblems;
using Xunit.Abstractions;

namespace Raijin.ProblemSolvingService.Domain.Tests.BooleanFormulas;

public class TseitinTransformerTests
{
    [Fact]
    public void GivenVariable_WhenTransforming_ThenReturnsAppropriateSatProblemAndSymbolTable()
    {
        var variable = new Variable("1");
        var tseitinTransformer = new TseitinTransformer();

        SatProblem satProblem = tseitinTransformer.Transform(new BooleanFormula(variable),
            out Dictionary<Variable, SatVariable> symbolTable);

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
        var tseitinTransformer = new TseitinTransformer();

        SatProblem satProblem = tseitinTransformer.Transform(new BooleanFormula(negation),
            out Dictionary<Variable, SatVariable> symbolTable);

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
        var tseitinTransformer = new TseitinTransformer();

        SatProblem satProblem = tseitinTransformer.Transform(new BooleanFormula(conjunction),
            out Dictionary<Variable, SatVariable> symbolTable);

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
    public void GivenDisjunction_WhenTransforming_ThenReturnsAppropriateSatProblemAndSymbolTable()
    {
        Disjunction disjunction = new Variable("1").Or(new Variable("2"));
        var tseitinTransformer = new TseitinTransformer();

        SatProblem satProblem = tseitinTransformer.Transform(new BooleanFormula(disjunction),
            out Dictionary<Variable, SatVariable> symbolTable);

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
    public void GivenEquivalence_WhenTransforming_ThenReturnsAppropriateSatProblemAndSymbolTable()
    {
        Equivalence equivalence = new Variable("1").Equal(new Variable("2"));
        var tseitinTransformer = new TseitinTransformer();

        SatProblem satProblem = tseitinTransformer.Transform(new BooleanFormula(equivalence),
            out Dictionary<Variable, SatVariable> symbolTable);

        satProblem.Clauses.Should().BeEquivalentTo([
            new Clause(Literal.FromInteger(1), Literal.FromInteger(-3)),
            new Clause(Literal.FromInteger(2), Literal.FromInteger(-3)),
            new Clause(Literal.FromInteger(-1), Literal.FromInteger(-2), Literal.FromInteger(3)),


            new Clause(Literal.FromInteger(1), Literal.FromInteger(4)),
            new Clause(Literal.FromInteger(-1), Literal.FromInteger(-4)),

            new Clause(Literal.FromInteger(2), Literal.FromInteger(5)),
            new Clause(Literal.FromInteger(-2), Literal.FromInteger(-5)),

            new Clause(Literal.FromInteger(4), Literal.FromInteger(-6)),
            new Clause(Literal.FromInteger(5), Literal.FromInteger(-6)),
            new Clause(Literal.FromInteger(-4), Literal.FromInteger(-5), Literal.FromInteger(6)),


            new Clause(Literal.FromInteger(-3), Literal.FromInteger(7)),
            new Clause(Literal.FromInteger(-6), Literal.FromInteger(7)),
            new Clause(Literal.FromInteger(3), Literal.FromInteger(6), Literal.FromInteger(-7))
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
        var tseitinTransformer = new TseitinTransformer();

        SatProblem satProblem = tseitinTransformer.Transform(new BooleanFormula(disjunction),
            out Dictionary<Variable, SatVariable> symbolTable);

        satProblem.Clauses.Should().BeEquivalentTo([
            new Clause(Literal.FromInteger(1), Literal.FromInteger(2)),
            new Clause(Literal.FromInteger(-1), Literal.FromInteger(-2)),

            new Clause(Literal.FromInteger(2), Literal.FromInteger(-4)),
            new Clause(Literal.FromInteger(3), Literal.FromInteger(-4)),
            new Clause(Literal.FromInteger(-2), Literal.FromInteger(-3), Literal.FromInteger(4)),


            new Clause(Literal.FromInteger(3), Literal.FromInteger(5)),
            new Clause(Literal.FromInteger(-3), Literal.FromInteger(-5)),

            new Clause(Literal.FromInteger(1), Literal.FromInteger(-6)),
            new Clause(Literal.FromInteger(5), Literal.FromInteger(-6)),
            new Clause(Literal.FromInteger(-1), Literal.FromInteger(-5), Literal.FromInteger(6)),


            new Clause(Literal.FromInteger(-4), Literal.FromInteger(7)),
            new Clause(Literal.FromInteger(-6), Literal.FromInteger(7)),
            new Clause(Literal.FromInteger(4), Literal.FromInteger(6), Literal.FromInteger(-7))
        ]);
        symbolTable.Should().BeEquivalentTo(new Dictionary<Variable, SatVariable>
        {
            [new Variable("1")] = new(1),
            [new Variable("2")] = new(3)
        });
    }

    [Fact]
    public void GivenImplication_WhenTransforming_ThenReturnsAppropriateSatProblemAndSymbolTable()
    {
        Implication disjunction = new Variable("1").Imply(new Variable("2"));
        var tseitinTransformer = new TseitinTransformer();

        SatProblem satProblem = tseitinTransformer.Transform(new BooleanFormula(disjunction),
            out Dictionary<Variable, SatVariable> symbolTable);

        satProblem.Clauses.Should().BeEquivalentTo([
            new Clause(Literal.FromInteger(1), Literal.FromInteger(2)),
            new Clause(Literal.FromInteger(-1), Literal.FromInteger(-2)),

            new Clause(Literal.FromInteger(-2), Literal.FromInteger(4)),
            new Clause(Literal.FromInteger(-3), Literal.FromInteger(4)),
            new Clause(Literal.FromInteger(2), Literal.FromInteger(3), Literal.FromInteger(-4))
        ]);
        symbolTable.Should().BeEquivalentTo(new Dictionary<Variable, SatVariable>
        {
            [new Variable("1")] = new(1),
            [new Variable("2")] = new(3)
        });
    }

    [Fact]
    public void GivenUnknownNode_WhenTransforming_ThenThrowsNotSupportedException()
    {
        var booleanExpression = Substitute.For<IBooleanExpression>();
        booleanExpression.Desugar().Returns(booleanExpression);
        var tseitinTransformer = new TseitinTransformer();

        Action when = () => tseitinTransformer.Transform(new BooleanFormula(booleanExpression),
            out Dictionary<Variable, SatVariable> _);

        when.Should().Throw<NotSupportedException>();
    }
}