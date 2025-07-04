using FluentAssertions;
using Njinx.ProblemSolvingService.Domain.SatProblems;

namespace Njinx.ProblemSolvingService.Domain.Tests.SatProblems;

public class LiteralTests
{
    [Fact]
    public void GivenVariableIsNull_WhenCreatingLiteral_ThenThrowArgumentNullException()
    {
        Variable variable = null!;

        Action when = () => _ = new Literal(variable);

        when.Should().Throw<ArgumentNullException>();
    }
}