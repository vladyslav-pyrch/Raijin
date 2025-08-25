using FluentAssertions;
using Raijin.ProblemSolvingService.Domain.BooleanFormulas;

namespace Raijin.ProblemSolvingService.Domain.Tests.BooleanFormulas;

[Trait("Category", "Unit")]
public class VariableTests
{
    [Fact]
    public void GiveValidIdAndName_WhenCreatingVariable_ThenVariableIsCreated()
    {
        var variable = new Variable("name");

        variable.Name.Should().BeEquivalentTo("name");
    }

    [Fact]
    public void GiveNameIsNull_WhenCreatingVariable_ThenThrowsArgumentNullException()
    {
        Action when = () => _ = new Variable(null!);

        when.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GiveNameIsEmpty_WhenCreatingVariable_ThenThrowsArgumentException()
    {
        Action when = () => _ = new Variable(string.Empty);

        when.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(" "), InlineData("\t"), InlineData("\n"), InlineData("\r")]
    public void GiveNameIsWhitespace_WhenCreatingVariable_ThenThrowsArgumentException(string name)
    {
        Action when = () => _ = new Variable(name);

        when.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GivenVariable_WhenDesugaring_ReturnsItself()
    {
        var variable = new Variable("name");

        var desugared = variable.Desugar();

        desugared.Should().Be(variable);
    }
}