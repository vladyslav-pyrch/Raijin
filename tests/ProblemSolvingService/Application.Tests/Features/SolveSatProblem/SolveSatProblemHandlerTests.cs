using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Raijin.ProblemSolvingService.Application.Features;
using Raijin.ProblemSolvingService.Application.Features.Dtos;
using Raijin.ProblemSolvingService.Application.Features.SolveSatProblem;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Application.Tests.Features.SolveSatProblem;

[Trait("Category", "Unit")]
public class SolveSatProblemHandlerTests
{
    [Fact]
    public async Task GivenInvalidCommandWithNoClauses_WhenHandling_ThenThrowsValidationException()
    {
        var command = new SolveSatProblemCommand(Clauses: []);

        var solver = Substitute.For<ISatSolver>();
        var handler = new SolveSatProblemHandler(solver);

        Func<Task> when = async () => await handler.Handle(command, CancellationToken.None);

        await when.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task GivenInvalidCommandWithEmptyLiterals_WhenHandling_ThenThrowsValidationException()
    {
        var command = new SolveSatProblemCommand(Clauses: [
            new ClauseDto(Literals: [])
        ]);

        var solver = Substitute.For<ISatSolver>();
        var handler = new SolveSatProblemHandler(solver);

        Func<Task> when = async () => await handler.Handle(command, CancellationToken.None);

        await when.Should().ThrowAsync<ValidationException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GivenInvalidCommandWithInvalidVariableNumber_WhenHandling_ThenThrowsValidationException(int variableNumber)
    {
        var command = new SolveSatProblemCommand(Clauses: [
            new ClauseDto(Literals: [
                new LiteralDto(VariableNumber: variableNumber, IsNegated: false)
            ])
        ]);

        var solver = Substitute.For<ISatSolver>();
        var handler = new SolveSatProblemHandler(solver);

        Func<Task> when = async () => await handler.Handle(command, CancellationToken.None);

        await when.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task GiveSolvableProblem_WhenHandling_ThenReturnsSatisfiableResult()
    {
        var command = new SolveSatProblemCommand(Clauses:
        [
            new ClauseDto(Literals: [new LiteralDto(VariableNumber: 1, IsNegated: false)])
        ]);

        var solver = Substitute.For<ISatSolver>();
        List<SatVariableAssignment> assignments = [new(new SatVariable(1), true)];
        solver.Solve(Arg.Any<SatProblem>(), Arg.Any<CancellationToken>())
            .Returns(SatResult.Solvable(assignments));

        var handler = new SolveSatProblemHandler(solver);

        SolveSatProblemResult result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeEquivalentTo(new SolveSatProblemResult(
            SolvingStatusDto.Satisfiable,
            VariableAssignments: [new SatVariableAssignmentDto(VariableNumber: 1, Assignment: true)]
        ));
        await solver.Received(1).Solve(Arg.Any<SatProblem>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GiveUnsolvableProblem_WhenHandling_ThenReturnsUnsatisfiableResult()
    {
        var command = new SolveSatProblemCommand(Clauses:
        [
            new ClauseDto(Literals: [new LiteralDto(VariableNumber: 1, IsNegated: false)]),
            new ClauseDto(Literals: [new LiteralDto(VariableNumber: 1, IsNegated: true)])
        ]);

        var solver = Substitute.For<ISatSolver>();
        solver.Solve(Arg.Any<SatProblem>(), Arg.Any<CancellationToken>())
            .Returns(SatResult.Unsolvable());

        var handler = new SolveSatProblemHandler(solver);

        SolveSatProblemResult result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeEquivalentTo(new SolveSatProblemResult(
            SolvingStatusDto.Unsatisfiable,
            VariableAssignments: []
        ));
        await solver.Received(1).Solve(Arg.Any<SatProblem>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenIndeterminateProblem_WhenHandling_ThenReturnsIndeterminateResult()
    {
        var command = new SolveSatProblemCommand(Clauses:
        [
            new ClauseDto(Literals: [new LiteralDto(VariableNumber: 1, IsNegated: false)])
        ]);

        var solver = Substitute.For<ISatSolver>();
        solver.Solve(Arg.Any<SatProblem>(), Arg.Any<CancellationToken>())
            .Returns(SatResult.Indeterminate());

        var handler = new SolveSatProblemHandler(solver);

        SolveSatProblemResult result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeEquivalentTo(new SolveSatProblemResult(
            SolvingStatusDto.Indeterminate,
            VariableAssignments: []
        ));
        await solver.Received(1).Solve(Arg.Any<SatProblem>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenCancellationToken_WhenHandling_ThenPassesToSolver()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = cancellationTokenSource.Token;
        var command = new SolveSatProblemCommand(Clauses:
        [
            new ClauseDto(Literals: [new LiteralDto(VariableNumber: 1, IsNegated: false)])
        ]);

        var solver = Substitute.For<ISatSolver>();
        List<SatVariableAssignment> assignments = [new(new SatVariable(1), true)];
        solver.Solve(Arg.Any<SatProblem>(), Arg.Any<CancellationToken>())
            .Returns(SatResult.Solvable(assignments));

        await new SolveSatProblemHandler(solver).Handle(command, cancellationToken);

        await solver.Received(1).Solve(Arg.Any<SatProblem>(), cancellationToken);
    }
}