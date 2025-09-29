using FluentAssertions;
using FluentResults;
using NSubstitute;
using Raijin.ProblemSolvingService.Application.Features;
using Raijin.ProblemSolvingService.Application.Features.Dtos;
using Raijin.ProblemSolvingService.Application.Features.SolveSatExpression;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Application.Tests.Features.SolveSatExpression;

[Trait("Category", "Unit")]
public class SolveSatExpressionHandlerTests
{
    [Fact]
    public async Task GivenSatisfiableExpression_WhenHandled_ThenReturnsSatisfiableResult()
    {
        var command = new SolveSatExpressionCommand("(a b) (a ~b) (b)");
        var solver = Substitute.For<ISatSolver>();
        solver.Solve(Arg.Any<SatProblem>(), Arg.Any<CancellationToken>())
            .Returns(SatResult.Solvable([
                SatVariableAssignment.FromInteger(1),
                SatVariableAssignment.FromInteger(2)
            ]));

        var handler = new SolveSatExpressionHandler(solver);

        Result<SolveSatExpressionResult> result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(new SolveSatExpressionResult(SolvingStatusDto.Satisfiable, [
            new NamedSatVariableAssignmentDto("a", true),
            new NamedSatVariableAssignmentDto("b", true)
        ]));
    }

    [Fact]
    public async Task GivenUnsatisfiableExpression_WhenHandled_ThenReturnsUnsolvableResult()
    {
        var command = new SolveSatExpressionCommand("(a b) (a ~b) (b) (~a)");
        var solver = Substitute.For<ISatSolver>();
        solver.Solve(Arg.Any<SatProblem>(), Arg.Any<CancellationToken>())
            .Returns(SatResult.Unsolvable());

        var handler = new SolveSatExpressionHandler(solver);

        Result<SolveSatExpressionResult> result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(new SolveSatExpressionResult(SolvingStatusDto.Unsatisfiable, []));
    }

    [Fact]
    public async Task GivenIndeterminateExpression_WhenHandled_ThenReturnsIndeterminateResult()
    {
        var command = new SolveSatExpressionCommand("(a b) (a ~b) (b) (~a)");
        var solver = Substitute.For<ISatSolver>();
        solver.Solve(Arg.Any<SatProblem>(), Arg.Any<CancellationToken>())
            .Returns(SatResult.Indeterminate());

        var handler = new SolveSatExpressionHandler(solver);

        Result<SolveSatExpressionResult> result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(new SolveSatExpressionResult(SolvingStatusDto.Indeterminate, []));
    }

    [Fact]
    public async Task GivenActiveCancellationToken_WhenHandled_ThenReturnsIndeterminateResult()
    {
        var command = new SolveSatExpressionCommand("(a b) (a ~b) (b) (~a)");
        var solver = Substitute.For<ISatSolver>();
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        solver.Solve(Arg.Any<SatProblem>(), Arg.Any<CancellationToken>())
            .Returns(SatResult.Indeterminate());
        var handler = new SolveSatExpressionHandler(solver);

        Result<SolveSatExpressionResult> result = await handler.Handle(command, cts.Token);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(new SolveSatExpressionResult(SolvingStatusDto.Indeterminate, []));
    }

    [Fact]
    public async Task GivenExpressionWithEmptyClause_WhenHandled_ThenReturnsSatParseErrorResult()
    {
        var command = new SolveSatExpressionCommand("(a b) () (b) (~a)");
        var solver = Substitute.For<ISatSolver>();
        var handler = new SolveSatExpressionHandler(solver);

        Result<SolveSatExpressionResult> result = await handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Should().BeOfType<SatParseError>().Which.Message.Should().Be("Error at 7; No empty clauses are allowed.");
        await solver.DidNotReceive().Solve(Arg.Any<SatProblem>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenExpressionWithUnclosedClause_WhenHandled_ThenReturnsSatParseErrorResult()
    {
        var command = new SolveSatExpressionCommand("(a b (a ~b) (b) (~a)");
        var solver = Substitute.For<ISatSolver>();
        var handler = new SolveSatExpressionHandler(solver);

        Result<SolveSatExpressionResult> result = await handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Should().BeOfType<SatParseError>().Which.Message.Should().Be("Error at 5; Expected ')'");
        await solver.DidNotReceive().Solve(Arg.Any<SatProblem>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenExpressionWithUnclosedClauseAtTheEnd_WhenHandled_ThenReturnsSatParseErrorResult()
    {
        var command = new SolveSatExpressionCommand("(a b) (a ~b) (b) (~a");
        var solver = Substitute.For<ISatSolver>();
        var handler = new SolveSatExpressionHandler(solver);

        Result<SolveSatExpressionResult> result = await handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Should().BeOfType<SatParseError>().Which.Message.Should().Be("Error at 20; Expected ')'");
        await solver.DidNotReceive().Solve(Arg.Any<SatProblem>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenExpressionWithOpeningBracketInsideClause_WhenHandled_ThenReturnsSatParseErrorResult()
    {
        var command = new SolveSatExpressionCommand("(a b) (a ( ~b) (b) (~a)");
        var solver = Substitute.For<ISatSolver>();
        var handler = new SolveSatExpressionHandler(solver);

        Result<SolveSatExpressionResult> result = await handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Should().BeOfType<SatParseError>().Which.Message.Should().Be("Error at 9; Expected ')'");
        await solver.DidNotReceive().Solve(Arg.Any<SatProblem>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenExpressionWithUnopenedClause_WhenHandled_ThenReturnsSatParseErrorResult()
    {
        var command = new SolveSatExpressionCommand("(a b) (a ) ~b) (b) (~a");
        var solver = Substitute.For<ISatSolver>();
        var handler = new SolveSatExpressionHandler(solver);

        Result<SolveSatExpressionResult> result = await handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Should().BeOfType<SatParseError>().Which.Message.Should().Be("Error at 11; Expected '('");
        await solver.DidNotReceive().Solve(Arg.Any<SatProblem>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenExpressionWithUnknownTokenInTheMiddle_WhenHandled_ThenReturnsSatParseErrorResult()
    {
        var command = new SolveSatExpressionCommand("(a b) (a ~b) |32 (b) (~a)");
        var solver = Substitute.For<ISatSolver>();
        var handler = new SolveSatExpressionHandler(solver);

        Result<SolveSatExpressionResult> result = await handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Should().BeOfType<SatParseError>().Which.Message.Should().Be("Error at 13; Unknown token '|32'");
        await solver.DidNotReceive().Solve(Arg.Any<SatProblem>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenExpressionWithUnknownTokenAtTheEnd_WhenHandled_ThenReturnsSatParseErrorResult()
    {
        var command = new SolveSatExpressionCommand("(a b) (a ~b) (b) (~a) \\fea3");
        var solver = Substitute.For<ISatSolver>();
        var handler = new SolveSatExpressionHandler(solver);

        Result<SolveSatExpressionResult> result = await handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Should().BeOfType<SatParseError>().Which.Message.Should().Be("Error at 22; Unknown token '\\fea3'");
        await solver.DidNotReceive().Solve(Arg.Any<SatProblem>(), Arg.Any<CancellationToken>());
    }
}