using FluentAssertions;
using FluentResults;
using NSubstitute;
using Raijin.ProblemSolvingService.Application.Features;
using Raijin.ProblemSolvingService.Application.Features.Dtos;
using Raijin.ProblemSolvingService.Application.Features.SolveBooleanExpression;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Application.Tests.Features.SolveBooleanExpression;

public class SolveBooleanExpressionHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnExpectedResult()
    {
        // Arrange
        var expression = "(A | B) & (~ A | C)";
        var request = new SolveBooleanExpressionCommand(expression);

        List<VariableAssignmentDto> expectedAssignments =
        [
            new("A", true),
            new("B", false),
            new("C", true)
        ];
        var expectedResult = new SolveBooleanExpressionResult(
            SolvingStatusDto.Satisfiable,
            expectedAssignments
        );

        var solver = Substitute.For<ISatSolver>();
        solver.Solve(Arg.Any<SatProblem>(), Arg.Any<CancellationToken>())
            .Returns(SatResult.Solvable([
                SatVariableAssignment.FromInteger(1),
                SatVariableAssignment.FromInteger(-2),
                SatVariableAssignment.FromInteger(5)
            ]));
        var handler = new SolveBooleanExpressionHandler(solver);

        // Act
        Result<SolveBooleanExpressionResult> result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Value.Should().BeEquivalentTo(expectedResult);
        await solver.Received(1).Solve(Arg.Any<SatProblem>(), Arg.Any<CancellationToken>());
    }
}