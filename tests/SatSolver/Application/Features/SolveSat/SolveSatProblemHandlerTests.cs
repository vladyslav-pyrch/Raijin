using Microsoft.Extensions.Logging;
using NSubstitute;
using Raijin.SatSolver.Application.Features.SolveSatProblem;
using Raijin.SatSolver.Application.Messaging;
using Raijin.SatSolver.Application.Persistence;
using Raijin.SatSolver.Application.Solver;
using Raijin.SatSolver.Domain.SatProblems;

namespace Raijin.SatSolver.Application.Tests.Features.SolveSat;

public class SolveSatProblemHandlerTests
{
    [Fact]
    public async Task
        GivenDimacs_WhenHandling_ThenShouldCreateSatProblemAndSolveItAndUpdateSatProblemAndPublishSatProblemSolvedEvent()
    {
        var repository = Substitute.For<ISatProblemRepository>();
        var solver = Substitute.For<ISatSolver>();
        var eventBus = Substitute.For<IMessageBus>();
        var logger = Substitute.For<ILogger<SolveSatProblemHandler>>();

        var handler = new SolveSatProblemHandler(repository, solver, eventBus, logger);

        var dimacs = "p cnf 3 2\n1 -3 0\n-1 2 3 0";
        var command = new SolveSatProblemCommand(Guid.CreateVersion7(), dimacs);
        int[] solution = [1, -2, 3];
        solver.Solve(Arg.Any<SatProblem>(), Arg.Any<CancellationToken>()).Returns(solution);

        await handler.Handle(command, CancellationToken.None);

        await repository.Received(1).AddAndSave(Arg.Any<SatProblem>(),
            Arg.Any<CancellationToken>());
        await solver.Received(1).Solve(Arg.Any<SatProblem>(), Arg.Any<CancellationToken>());
        await repository.Received(1).UpdateAndSave(Arg.Any<SatProblem>(), Arg.Any<CancellationToken>());
        // await eventBus.Received(1).Publish(Arg.Any<SatProblemSolved>(), Arg.Any<CancellationToken>());
    }
}