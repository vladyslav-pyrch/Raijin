using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Raijin.ProblemSolvingService.Application.Cqrs;
using Raijin.ProblemSolvingService.Application.Features.CommonSat.Commands.SolveSatProblem;
using Raijin.ProblemSolvingService.Application.Features.CommonSat.Commands.SolveSatProblemInternal;
using Raijin.ProblemSolvingService.Application.Features.CommonSat.Dtos;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Application.Tests.Features.CommonSat.Commands.SolveSatProblem;

public class SolveSatProblemCommandHandlerTests
{
    [Fact]
    public async Task GivenValidCommand_WhenHandling_ThenDispatchesSolveSatProblemInternalCommand()
    {
        // Arrange
        var command = new SolveSatProblemCommand(Clauses: [
            new ClauseDto(Literals: [
                new LiteralDto(VariableNumber: 1, IsNegated: false)
            ])
        ]);

        var dispatcher = Substitute.For<ICommandDispatcher>();
        dispatcher.Dispatch<SolveSatProblemInternalCommand, SatResult>(
                command: Arg.Any<SolveSatProblemInternalCommand>(),
                cancellationToken: Arg.Any<CancellationToken>()
            ).Returns(SatResult.Solvable([VariableAssignment.FromInteger(1)]));

        var handler = new SolveSatProblemCommandHandler(dispatcher);

        // Act
        SolveSatProblemCommandResult result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(new SolveSatProblemCommandResult(
            SolvingStatusDto.Satisfiable,
            VariableAssignments: [
                new VariableAssignmentDto(VariableNumber: 1, Assignment: true)
            ]
        ));
        await dispatcher.Received(1).Dispatch<SolveSatProblemInternalCommand, SatResult>(
            command: Arg.Is<SolveSatProblemInternalCommand>(internalCommand =>
                internalCommand.SatProblem.GetNumberOfClauses() == 1 &&
                internalCommand.SatProblem.GetNumberOfVariables() == 1
            ),
            cancellationToken: Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task GivenInvalidCommandWithNoClauses_WhenHandling_ThenThrowsValidationException()
    {
        // Arrange
        var command = new SolveSatProblemCommand(Clauses: []);

        var dispatcher = Substitute.For<ICommandDispatcher>();
        dispatcher.Dispatch<SolveSatProblemInternalCommand, SatResult>(
            command: Arg.Any<SolveSatProblemInternalCommand>(),
            cancellationToken: Arg.Any<CancellationToken>()
        )!.Returns((SatResult)null!);

        var handler = new SolveSatProblemCommandHandler(dispatcher);

        // Act
        Func<Task> when = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ValidationException>(when);
    }

    [Fact]
    public async Task GivenInvalidCommandWithEmptyLiterals_WhenHandling_ThenThrowsValidationException()
    {
        // Arrange
        var command = new SolveSatProblemCommand(Clauses: [
            new ClauseDto(Literals: [])
        ]);

        var dispatcher = Substitute.For<ICommandDispatcher>();
        dispatcher.Dispatch<SolveSatProblemInternalCommand, SatResult>(
            command: Arg.Any<SolveSatProblemInternalCommand>(),
            cancellationToken: Arg.Any<CancellationToken>()
        )!.Returns((SatResult)null!);

        var handler = new SolveSatProblemCommandHandler(dispatcher);

        // Act
        Func<Task> when = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ValidationException>(when);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GivenInvalidCommandWithInvalidVariableNumber_WhenHandling_ThenThrowsValidationException(int variableNumber)
    {
        // Arrange
        var command = new SolveSatProblemCommand(Clauses: [
            new ClauseDto(Literals: [
                new LiteralDto(VariableNumber: variableNumber, IsNegated: false)
            ])
        ]);

        var dispatcher = Substitute.For<ICommandDispatcher>();
        dispatcher.Dispatch<SolveSatProblemInternalCommand, SatResult>(
            command: Arg.Any<SolveSatProblemInternalCommand>(),
            cancellationToken: Arg.Any<CancellationToken>()
        )!.Returns((SatResult)null!);

        var handler = new SolveSatProblemCommandHandler(dispatcher);

        // Act
        Func<Task> when = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ValidationException>(when);
    }
}