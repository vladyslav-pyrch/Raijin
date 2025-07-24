using FluentAssertions;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Domain.Tests.SatProblems;

public class VariableAssignmentTests
{
    [Fact]
    public void GivenPositiveInteger_WhenCreatingFromInteger_ThenCreatesTrueAssignment()
    {
        const int value = 1;

        VariableAssignment assignment = VariableAssignment.FromInteger(value);

        assignment.Should().Be(new VariableAssignment(new Variable(1), true));
    }

    [Fact]
    public void GivenNegativeInteger_WhenCreatingFromInteger_ThenCreatesFalseAssignment()
    {
        const int value = -1;

        var assignment = VariableAssignment.FromInteger(value);

        assignment.Should().Be(new VariableAssignment(new Variable(1), false));
    }

    [Fact]
    public void GivenNullVariable_WhenCreatingVariableAssignment_ThenThrowsArgumentNullException()
    {
        Variable variable = null!;

        Action when = () => _ = new VariableAssignment(variable, true);

        when.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GivenIntegerZero_WhenCreatingFromInteger_ThenThrowsArgumentOutOfRangeException()
    {
        const int value = 0;

        Action when = () => _ = VariableAssignment.FromInteger(value);

        when.Should().Throw<ArgumentOutOfRangeException>();
    }
}