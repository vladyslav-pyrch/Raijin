using FluentAssertions;
using NSubstitute;
using Raijin.ProblemSolvingService.Application.Features.CommonSat;
using Raijin.ProblemSolvingService.Application.Features.CommonSat.Commands.SolveSatProblem;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Application.Tests.Features.CommonSat.Commands.SolveSatProblem;

public class SolveSatProblemInternalCommandTests
{
    [Fact]
    public async Task GivenSolvableProblem_WhenHandling_ThenReturnsSolvableResult()
    {
        var solvableProblem = new SatProblem();
        solvableProblem.AddClause([Literal.FromInteger(-1), Literal.FromInteger(2)]);
        solvableProblem.AddClause([Literal.FromInteger(1)]);

        List<VariableAssignment> solution = [VariableAssignment.FromInteger(1), VariableAssignment.FromInteger(2)];

        var satSolver = Substitute.For<ISatSolver>();

        satSolver.Solve(Arg.Is(solvableProblem), Arg.Any<CancellationToken>())
            .Returns(SatResult.Solvable(solution));

        var handler = new SolveSatProblemInternalCommandHandler(satSolver);
        var command = new SolveSatProblemInternalCommand(solvableProblem);

        SatResult result = await handler.Handle(command, CancellationToken.None);

        result.Should().Be(SatResult.Solvable(solution));
        await satSolver.Received(1).Solve(Arg.Is(solvableProblem), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenUnsolvableProblem_WhenHandling_ThenReturnsUnsolvableResult()
    {
        var unsolvableProblem = new SatProblem();
        unsolvableProblem.AddClause([Literal.FromInteger(1)]);
        unsolvableProblem.AddClause([Literal.FromInteger(-1)]);

        var satSolver = Substitute.For<ISatSolver>();
        satSolver.Solve(Arg.Is(unsolvableProblem), Arg.Any<CancellationToken>())
            .Returns(SatResult.Unsolvable());

        var handler = new SolveSatProblemInternalCommandHandler(satSolver);
        var command = new SolveSatProblemInternalCommand(unsolvableProblem);

        SatResult result = await handler.Handle(command, CancellationToken.None);

        result.Should().Be(SatResult.Unsolvable());
        await satSolver.Received(1).Solve(Arg.Is(unsolvableProblem), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenIndeterminateProblem_WhenHandling_ThenReturnsIndeterminateResult()
    {
        var indeterminateProblem = new SatProblem();
        indeterminateProblem.AddClause([Literal.FromInteger(1), Literal.FromInteger(3)]);

        var satSolver = Substitute.For<ISatSolver>();
        satSolver.Solve(Arg.Is(indeterminateProblem), Arg.Any<CancellationToken>())
            .Returns(SatResult.Indeterminate());

        var handler = new SolveSatProblemInternalCommandHandler(satSolver);
        var command = new SolveSatProblemInternalCommand(indeterminateProblem);

        SatResult result = await handler.Handle(command, CancellationToken.None);

        result.Should().Be(SatResult.Indeterminate());
        await satSolver.Received(1).Solve(Arg.Is(indeterminateProblem), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenCancellationToken_WhenHandling_ThenPassesCancellationTokenToSolver()
    {
        using var cts = new CancellationTokenSource();

        var satSolver = Substitute.For<ISatSolver>();
        satSolver.Solve(Arg.Any<SatProblem>(), Arg.Any<CancellationToken>())
            .Returns(async x =>
            {
                var cancellationToken = x.Arg<CancellationToken>();
                await Task.Delay(100, cancellationToken);
                return SatResult.Indeterminate();
            });

        var handler = new SolveSatProblemInternalCommandHandler(satSolver);
        var command = new SolveSatProblemInternalCommand(new SatProblem());

        Func<Task> when = async () =>
        {
            await cts.CancelAsync();
            await handler.Handle(command, cts.Token);
        };

        await when.Should().ThrowAsync<OperationCanceledException>();
        await satSolver.Received(1).Solve(Arg.Any<SatProblem>(), Arg.Is(cts.Token));
    }
}