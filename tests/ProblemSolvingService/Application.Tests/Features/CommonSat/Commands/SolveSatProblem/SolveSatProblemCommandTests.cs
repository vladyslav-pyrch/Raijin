using Castle.Core;
using FluentAssertions;
using Moq;
using Raijin.ProblemSolvingService.Application.Features.CommonSat;
using Raijin.ProblemSolvingService.Application.Features.CommonSat.Commands.SolveSatProblem;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Application.Tests.Features.CommonSat.Commands.SolveSatProblem;

public class SolveSatProblemCommandTests
{
    [Fact]
    public async Task GivenSolvableProblem_WhenHandling_ThenReturnsSolvableResult()
    {
        var solvableProblem = new SatProblem();
        solvableProblem.AddClause([Literal.FromInteger(-1), Literal.FromInteger(2)]);
        solvableProblem.AddClause([Literal.FromInteger(1)]);

        List<VariableAssignment> solution = [VariableAssignment.FromInteger(1), VariableAssignment.FromInteger(2)];

        var satSolverMock = new Mock<ISatSolver>();
        satSolverMock
            .Setup(s => s.Solve(
                It.Is(solvableProblem, ReferenceEqualityComparer<SatProblem>.Instance),
                It.IsAny<CancellationToken>())
            ).ReturnsAsync(SatResult.Solvable(solution))
            .Verifiable(Times.Once);

        ISatSolver satSolver = satSolverMock.Object;

        var handler = new SolveSatProblemCommandHandler(satSolver);
        var command = new SolveSatProblemCommand(solvableProblem);

        SatResult result = await handler.Handle(command, CancellationToken.None);

        result.Should().Be(SatResult.Solvable(solution));
        satSolverMock.Verify();
    }

    [Fact]
    public async Task GivenUnsolvableProblem_WhenHandling_ThenReturnsUnsolvableResult()
    {
        var unsolvableProblem = new SatProblem();
        unsolvableProblem.AddClause([Literal.FromInteger(1)]);
        unsolvableProblem.AddClause([Literal.FromInteger(-1)]);

        var satSolverMock = new Mock<ISatSolver>();
        satSolverMock
            .Setup(s => s.Solve(
                It.Is(unsolvableProblem, ReferenceEqualityComparer<SatProblem>.Instance),
                It.IsAny<CancellationToken>())
            ).ReturnsAsync(SatResult.Unsolvable())
            .Verifiable(Times.Once);

        ISatSolver satSolver = satSolverMock.Object;

        var handler = new SolveSatProblemCommandHandler(satSolver);
        var command = new SolveSatProblemCommand(unsolvableProblem);

        SatResult result = await handler.Handle(command, CancellationToken.None);

        result.Should().Be(SatResult.Unsolvable());
        satSolverMock.Verify();
    }

    [Fact]
    public async Task GivenIndeterminateProblem_WhenHandling_ThenReturnsIndeterminateResult()
    {
        var indeterminateProblem = new SatProblem();
        indeterminateProblem.AddClause([Literal.FromInteger(1), Literal.FromInteger(3)]);

        var satSolverMock = new Mock<ISatSolver>();

        satSolverMock
            .Setup(s => s.Solve(
                It.Is(indeterminateProblem, ReferenceEqualityComparer<SatProblem>.Instance),
                It.IsAny<CancellationToken>())
            ).ReturnsAsync(SatResult.Indeterminate())
            .Verifiable(Times.Once);

        ISatSolver satSolver = satSolverMock.Object;

        var handler = new SolveSatProblemCommandHandler(satSolver);
        var command = new SolveSatProblemCommand(indeterminateProblem);

        SatResult result = await handler.Handle(command, CancellationToken.None);

        result.Should().Be(SatResult.Indeterminate());
        satSolverMock.Verify();
    }

    [Fact]
    public async Task GivenCancellationToken_WhenHandling_ThenPassesCancellationTokenToSolver()
    {
        using var cts = new CancellationTokenSource();

        var satSolverMock = new Mock<ISatSolver>();
        satSolverMock
            .Setup(s => s.Solve(
                It.IsAny<SatProblem>(),
                It.IsAny<CancellationToken>())
            ).ReturnsAsync((SatProblem _, CancellationToken cancellationToken) =>
            {
                Task.Delay(100, cancellationToken).Wait(cancellationToken);

                return SatResult.Indeterminate();
            }).Verifiable(Times.Once);

        ISatSolver satSolver = satSolverMock.Object;

        var handler = new SolveSatProblemCommandHandler(satSolver);
        var command = new SolveSatProblemCommand(new SatProblem());

        Func<Task> when = async () =>
        {
            await cts.CancelAsync();
            await handler.Handle(command, cts.Token);
        };

        await when.Should().ThrowAsync<OperationCanceledException>();
        satSolverMock.Verify();
    }
}