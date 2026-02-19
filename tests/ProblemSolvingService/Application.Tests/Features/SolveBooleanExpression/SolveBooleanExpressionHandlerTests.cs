using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Raijin.ProblemSolvingService.Application.Features;
using Raijin.ProblemSolvingService.Application.Features.Dtos;
using Raijin.ProblemSolvingService.Application.Features.SolveBooleanExpression;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Application.Tests.Features.SolveBooleanExpression;

[Trait("Category", "Unit")]
public class SolveBooleanExpressionHandlerTests(ITestOutputHelper outputHelper)
{
    private readonly ISatSolver _solver = Substitute.For<ISatSolver>();
    
    private readonly ILogger<SolveBooleanExpressionHandler> _logger = Substitute.For<ILogger<SolveBooleanExpressionHandler>>();

    [Fact]
    public async Task GivenExpression_WhenHandled_ThenReturnsSuccessResult()
    {
        _solver.Solve(Arg.Any<SatProblem>(), Arg.Any<CancellationToken>())
            .Returns(SatResult.Solvable([
                    SatVariableAssignment.FromInteger(1),
                    SatVariableAssignment.FromInteger(2),
                    SatVariableAssignment.FromInteger(3),
                    SatVariableAssignment.FromInteger(4),
                    SatVariableAssignment.FromInteger(-5),
                    SatVariableAssignment.FromInteger(-6),
                    SatVariableAssignment.FromInteger(7),
                    SatVariableAssignment.FromInteger(8),
                    SatVariableAssignment.FromInteger(9)
                ])
            );

        var command = new SolveBooleanExpressionCommand("a & b => (c ~| ~d)");
        var handler = new SolveBooleanExpressionHandler(_solver);

        Result<SolveBooleanExpressionResult> result = await handler.Handle(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.SolvingStatus.Should().Be(SolvingStatusDto.Solvable);
        result.Value.VariableAssignments.Should().BeEquivalentTo([
            new VariableAssignmentDto("a", true),
            new VariableAssignmentDto("b", true),
            new VariableAssignmentDto("c", true),
            new VariableAssignmentDto("d", false)
        ]);
    }

    [Fact]
    public async Task GivenEmptyExpression_WhenHandled_ThenReturnsFailureResult() =>
        await ExpectedErrorResultForExpression("");

    [Fact]
    public async Task GivenExpressionWithAnUnknownToken_WhenHandled_ThenReturnsFailureResult() =>
        await ExpectedErrorResultForExpression(">a & b");

    [Fact]
    public async Task GivenNotTokenIsAtTheEnd_WhenHandled_ThenReturnsFailureResult() =>
        await ExpectedErrorResultForExpression("a & b ~");

    [Theory]
    [InlineData("~&")]
    [InlineData("~|")]
    [InlineData("=>")]
    [InlineData("<=")]
    [InlineData("<=>")]
    [InlineData("^")]
    public async Task GivenNotTokenIsBeforeNonNegatableOperator__WhenHandled_ThenReturnsFailureResult(string @operator) =>
        await ExpectedErrorResultForExpression($"a ~ {@operator} b");

    [Fact]
    public async Task GivenTwoOperatorsAreAdjacent_WhenHandled_ThenReturnsFailureResult() =>
        await ExpectedErrorResultForExpression("a & * b");

    [Fact]
    public async Task GivenMismatchedLeftParentheses_WhenHandled_ThenReturnsFailureResult() =>
        await ExpectedErrorResultForExpression("(a & b");

    [Fact]
    public async Task GivenMismatchedRightParentheses_WhenHandled_ThenReturnsFailureResult() =>
        await ExpectedErrorResultForExpression("a & b)");

    [Fact]
    public async Task GivenNotEnoughOperandsForNotOperator_WhenHandled_ThenReturnsFailureResult() =>
        await ExpectedErrorResultForExpression("~()");

    [Fact]
    public async Task GivenNotEnoughOperandsForAndOperator_WhenHandled_ThenReturnsFailureResult() =>
        await ExpectedErrorResultForExpression("a & ");

    [Fact]
    public async Task GivenNotEnoughOperandsForNandOperator_WhenHandled_ThenReturnsFailureResult() =>
        await ExpectedErrorResultForExpression("a ~& ");

    [Fact]
    public async Task GivenNotEnoughOperandsForOrOperator_WhenHandled_ThenReturnsFailureResult() =>
        await ExpectedErrorResultForExpression("a | ");

    [Fact]
    public async Task GivenNotEnoughOperandsForNorOperator_WhenHandled_ThenReturnsFailureResult() =>
        await ExpectedErrorResultForExpression("a ~| ");

    [Fact]
    public async Task GivenNotEnoughOperandsForImplicationOperator_WhenHandled_ThenReturnsFailureResult() =>
        await ExpectedErrorResultForExpression("a => ");

    [Fact]
    public async Task GivenNotEnoughOperandsForImplicationBackwardOperator_WhenHandled_ThenReturnsFailureResult() =>
        await ExpectedErrorResultForExpression("a <= ");

    [Fact]
    public async Task GivenNotEnoughOperandsForEquivalenceOperator_WhenHandled_ThenReturnsFailureResult() =>
        await ExpectedErrorResultForExpression("a <=> ");

    [Fact]
    public async Task GivenNotEnoughOperandsForXorOperator_WhenHandled_ThenReturnsFailureResult() =>
        await ExpectedErrorResultForExpression("a ^ ");

    [Fact]
    public async Task GivenVariableExpression_WhenHandled_ThenReturnsSuccessResult()
    {
        _solver.Solve(Arg.Any<SatProblem>(), Arg.Any<CancellationToken>())
            .Returns(SatResult.Solvable([
                    SatVariableAssignment.FromInteger(1)
                ])
            );

        var command = new SolveBooleanExpressionCommand("a");
        var handler = new SolveBooleanExpressionHandler(_solver);

        Result<SolveBooleanExpressionResult> result = await handler.Handle(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.SolvingStatus.Should().Be(SolvingStatusDto.Solvable);
        result.Value.VariableAssignments.Should().BeEquivalentTo([
            new VariableAssignmentDto("a", true)
        ]);
    }

    [Fact]
    public async Task GivenNegatedVariableExpression_WhenHandled_ThenReturnsSuccessResult()
    {
        _solver.Solve(Arg.Any<SatProblem>(), Arg.Any<CancellationToken>())
            .Returns(SatResult.Solvable([
                    SatVariableAssignment.FromInteger(-1)
                ])
            );

        var command = new SolveBooleanExpressionCommand("~a");
        var handler = new SolveBooleanExpressionHandler(_solver);

        Result<SolveBooleanExpressionResult> result = await handler.Handle(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.SolvingStatus.Should().Be(SolvingStatusDto.Solvable);
        result.Value.VariableAssignments.Should().BeEquivalentTo([
            new VariableAssignmentDto("a", false)
        ]);
    }

    [Fact]
    public async Task GivenNegatedParenthesis_WhenHandled_ThenReturnsSuccessResult()
    {
        _solver.Solve(Arg.Any<SatProblem>(), Arg.Any<CancellationToken>())
            .Returns(SatResult.Solvable([
                    SatVariableAssignment.FromInteger(-1)
                ])
            );

        var command = new SolveBooleanExpressionCommand("~(a)");
        var handler = new SolveBooleanExpressionHandler(_solver);

        Result<SolveBooleanExpressionResult> result = await handler.Handle(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.SolvingStatus.Should().Be(SolvingStatusDto.Solvable);
        result.Value.VariableAssignments.Should().BeEquivalentTo([
            new VariableAssignmentDto("a", false)
        ]);
    }

    [Fact]
    public async Task GivenDoubleNegatedVariableExpression_WhenHandled_ThenReturnsSuccessResult()
    {
        _solver.Solve(Arg.Any<SatProblem>(), Arg.Any<CancellationToken>())
            .Returns(SatResult.Solvable([
                    SatVariableAssignment.FromInteger(1)
                ])
            );

        var command = new SolveBooleanExpressionCommand("~~a");
        var handler = new SolveBooleanExpressionHandler(_solver);

        Result<SolveBooleanExpressionResult> result = await handler.Handle(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.SolvingStatus.Should().Be(SolvingStatusDto.Solvable);
        result.Value.VariableAssignments.Should().BeEquivalentTo([
            new VariableAssignmentDto("a", true)
        ]);
    }

    [Fact]
    public async Task GivenAndExpression_WhenHandled_ThenReturnsSuccessResult()
    {
        _solver.Solve(Arg.Any<SatProblem>(), Arg.Any<CancellationToken>())
            .Returns(SatResult.Solvable([
                    SatVariableAssignment.FromInteger(1),
                    SatVariableAssignment.FromInteger(2)
                ])
            );

        var command = new SolveBooleanExpressionCommand("a & b");
        var handler = new SolveBooleanExpressionHandler(_solver);

        Result<SolveBooleanExpressionResult> result = await handler.Handle(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.SolvingStatus.Should().Be(SolvingStatusDto.Solvable);
        result.Value.VariableAssignments.Should().BeEquivalentTo([
            new VariableAssignmentDto("a", true),
            new VariableAssignmentDto("b", true)
        ]);
    }

    [Fact]
    public async Task GivenNandExpression_WhenHandled_ThenReturnsSuccessResult()
    {
        _solver.Solve(Arg.Any<SatProblem>(), Arg.Any<CancellationToken>())
            .Returns(SatResult.Solvable([
                    SatVariableAssignment.FromInteger(1),
                    SatVariableAssignment.FromInteger(-2)
                ])
            );

        var command = new SolveBooleanExpressionCommand("a ~& b");
        var handler = new SolveBooleanExpressionHandler(_solver);

        Result<SolveBooleanExpressionResult> result = await handler.Handle(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.SolvingStatus.Should().Be(SolvingStatusDto.Solvable);
        result.Value.VariableAssignments.Should().BeEquivalentTo([
            new VariableAssignmentDto("a", true),
            new VariableAssignmentDto("b", false)
        ]);
    }

    [Fact]
    public async Task GivenOrExpression_WhenHandled_ThenReturnsSuccessResult()
    {
        _solver.Solve(Arg.Any<SatProblem>(), Arg.Any<CancellationToken>())
            .Returns(SatResult.Solvable([
                    SatVariableAssignment.FromInteger(1),
                    SatVariableAssignment.FromInteger(-2)
                ])
            );

        var command = new SolveBooleanExpressionCommand("a | b");
        var handler = new SolveBooleanExpressionHandler(_solver);

        Result<SolveBooleanExpressionResult> result = await handler.Handle(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.SolvingStatus.Should().Be(SolvingStatusDto.Solvable);
        result.Value.VariableAssignments.Should().BeEquivalentTo([
            new VariableAssignmentDto("a", true),
            new VariableAssignmentDto("b", false)
        ]);
    }

    [Fact]
    public async Task GivenNorExpression_WhenHandled_ThenReturnsSuccessResult()
    {
        _solver.Solve(Arg.Any<SatProblem>(), Arg.Any<CancellationToken>())
            .Returns(SatResult.Solvable([
                    SatVariableAssignment.FromInteger(-1),
                    SatVariableAssignment.FromInteger(-2)
                ])
            );

        var command = new SolveBooleanExpressionCommand("a ~| b");
        var handler = new SolveBooleanExpressionHandler(_solver);

        Result<SolveBooleanExpressionResult> result = await handler.Handle(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.SolvingStatus.Should().Be(SolvingStatusDto.Solvable);
        result.Value.VariableAssignments.Should().BeEquivalentTo([
            new VariableAssignmentDto("a", false),
            new VariableAssignmentDto("b", false)
        ]);
    }

    [Fact]
    public async Task GivenImplicationExpression_WhenHandled_ThenReturnsSuccessResult()
    {
        _solver.Solve(Arg.Any<SatProblem>(), Arg.Any<CancellationToken>())
            .Returns(SatResult.Solvable([
                    SatVariableAssignment.FromInteger(-1),
                    SatVariableAssignment.FromInteger(2)
                ])
            );

        var command = new SolveBooleanExpressionCommand("a => b");
        var handler = new SolveBooleanExpressionHandler(_solver);

        Result<SolveBooleanExpressionResult> result = await handler.Handle(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.SolvingStatus.Should().Be(SolvingStatusDto.Solvable);
        result.Value.VariableAssignments.Should().BeEquivalentTo([
            new VariableAssignmentDto("a", false),
            new VariableAssignmentDto("b", true)
        ]);
    }

    [Fact]
    public async Task GivenImplicationBackwardExpression_WhenHandled_ThenReturnsSuccessResult()
    {
        _solver.Solve(Arg.Any<SatProblem>(), Arg.Any<CancellationToken>())
            .Returns(SatResult.Solvable([
                    SatVariableAssignment.FromInteger(1),
                    SatVariableAssignment.FromInteger(-2)
                ])
            );

        var command = new SolveBooleanExpressionCommand("a <= b");
        var handler = new SolveBooleanExpressionHandler(_solver);

        Result<SolveBooleanExpressionResult> result = await handler.Handle(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.SolvingStatus.Should().Be(SolvingStatusDto.Solvable);
        result.Value.VariableAssignments.Should().BeEquivalentTo([
            new VariableAssignmentDto("b", true),
            new VariableAssignmentDto("a", false)
        ]);
    }

    [Fact]
    public async Task GivenEquivalenceExpression_WhenHandled_ThenReturnsSuccessResult()
    {
        _solver.Solve(Arg.Any<SatProblem>(), Arg.Any<CancellationToken>())
            .Returns(SatResult.Solvable([
                    SatVariableAssignment.FromInteger(1),
                    SatVariableAssignment.FromInteger(2)
                ])
            );

        var command = new SolveBooleanExpressionCommand("a <=> b");
        var handler = new SolveBooleanExpressionHandler(_solver);

        Result<SolveBooleanExpressionResult> result = await handler.Handle(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.SolvingStatus.Should().Be(SolvingStatusDto.Solvable);
        result.Value.VariableAssignments.Should().BeEquivalentTo([
            new VariableAssignmentDto("a", true),
            new VariableAssignmentDto("b", true)
        ]);
    }

    [Fact]
    public async Task GivenXorExpression_WhenHandled_ThenReturnsSuccessResult()
    {
        _solver.Solve(Arg.Any<SatProblem>(), Arg.Any<CancellationToken>())
            .Returns(SatResult.Solvable([
                    SatVariableAssignment.FromInteger(1),
                    SatVariableAssignment.FromInteger(-2)
                ])
            );

        var command = new SolveBooleanExpressionCommand("a ^ b");
        var handler = new SolveBooleanExpressionHandler(_solver);

        Result<SolveBooleanExpressionResult> result = await handler.Handle(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.SolvingStatus.Should().Be(SolvingStatusDto.Solvable);
        result.Value.VariableAssignments.Should().BeEquivalentTo([
            new VariableAssignmentDto("a", true),
            new VariableAssignmentDto("b", false)
        ]);
    }

    [Fact]
    public async Task GivenUnsolvableExpression_WhenHandled_ThenReturnsSuccessResultWithUnsolvableStatus()
    {
        _solver.Solve(Arg.Any<SatProblem>(), Arg.Any<CancellationToken>())
            .Returns(SatResult.Unsolvable());

        var command = new SolveBooleanExpressionCommand("a & ~a");
        var handler = new SolveBooleanExpressionHandler(_solver);

        Result<SolveBooleanExpressionResult> result = await handler.Handle(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.SolvingStatus.Should().Be(SolvingStatusDto.Unsolvable);
        result.Value.VariableAssignments.Should().BeEmpty();
    }

    [Fact]
    public async Task GivenIndeterminateExpression_WhenHandled_ThenReturnsSuccessResultWithIndeterminateStatus()
    {
        _solver.Solve(Arg.Any<SatProblem>(), Arg.Any<CancellationToken>())
            .Returns(SatResult.Indeterminate());

        var command = new SolveBooleanExpressionCommand("a | ~a");
        var handler = new SolveBooleanExpressionHandler(_solver);

        Result<SolveBooleanExpressionResult> result = await handler.Handle(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.SolvingStatus.Should().Be(SolvingStatusDto.Indeterminate);
        result.Value.VariableAssignments.Should().BeEmpty();
    }

    private async Task ExpectedErrorResultForExpression(string expression)
    {
        var command = new SolveBooleanExpressionCommand(expression);
        var handler = new SolveBooleanExpressionHandler(_solver);

        Result<SolveBooleanExpressionResult> result = await handler.Handle(command);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().BeOfType<BooleanExpressionParseError>();
        outputHelper.WriteLine(result.Errors.First().Message);
    }

}