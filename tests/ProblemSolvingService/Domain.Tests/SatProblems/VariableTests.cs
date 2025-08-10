using FluentAssertions;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Domain.Tests.SatProblems;

[Trait("Category", "Unit")]
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