using FluentAssertions;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Domain.Tests.SatProblems;

[Trait("Category", "Unit")]
public class SatVariableAssignmentTests
{
    [Fact]
    public void GivenPositiveInteger_WhenCreatingFromInteger_ThenCreatesTrueAssignment()
    {
        const int value = 1;

        SatVariableAssignment assignment = SatVariableAssignment.FromInteger(value);

        assignment.Should().Be(new SatVariableAssignment(new SatVariable(1), true));
    }

    [Fact]
    public void GivenNegativeInteger_WhenCreatingFromInteger_ThenCreatesFalseAssignment()
    {
        const int value = -1;

        var assignment = SatVariableAssignment.FromInteger(value);

        assignment.Should().Be(new SatVariableAssignment(new SatVariable(1), false));
    }

    [Fact]
    public void GivenNullVariable_WhenCreatingVariableAssignment_ThenThrowsArgumentNullException()
    {
        SatVariable satVariable = null!;

        Action when = () => _ = new SatVariableAssignment(satVariable, true);

        when.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GivenIntegerZero_WhenCreatingFromInteger_ThenThrowsArgumentOutOfRangeException()
    {
        const int value = 0;

        Action when = () => _ = SatVariableAssignment.FromInteger(value);

        when.Should().Throw<ArgumentOutOfRangeException>();
    }
}