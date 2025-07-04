using FluentAssertions;
using Njinx.ProblemSolvingService.Domain.SatProblems;

namespace Njinx.ProblemSolvingService.Domain.Tests.SatProblems;

public class VariableTests
{
    [Fact]
    public void GivenNegativeId_WhenCreatingVariable_ThenThrowArgumentOutOfRangeException()
    {
        const int id = -1;

        Action when = () => _ = new Variable(id);

        when.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GivenIdIsZero_WhenCreatingVariable_ThenThrowArgumentOutOfRangeException()
    {
        const int id = 0;

        Action when = () => _ = new Variable(id);

        when.Should().Throw<ArgumentOutOfRangeException>();
    }
}