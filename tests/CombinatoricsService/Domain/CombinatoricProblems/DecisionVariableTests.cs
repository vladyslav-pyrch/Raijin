using FluentAssertions;
using Raijin.CombinatoricsService.Domain.CombinatoricProblems;

namespace Raijin.CombinatoricsService.Domain.Tests.CombinatoricProblems;

public class DecisionVariableTests
{
    [Fact]
    public void GivenNameAndStates_WhenConstructingDecisionVariable_ThenPropertiesAreSetCorrectly()
    {
        // Arrange
        const string name = "Color";
        List<string> states = ["Red", "Green", "Blue"];

        // Act
        var decisionVariable = new DecisionVariable(name, states);

        // Assert
        decisionVariable.Name.Should().Be(name);
        decisionVariable.States.Should().BeEquivalentTo(states);
    }

    [Fact]
    public void GivenMutableStates_WhenConstructingDecisionVariable_ThenStatesAreCopied()
    {
        // Arrange
        List<string> states = ["Small", "Medium", "Large"];
        var decisionVariable = new DecisionVariable("Size", states);

        // Act
        states.Add("ExtraLarge");

        // Assert
        decisionVariable.States.Should().BeEquivalentTo(["Small", "Medium", "Large"]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void GivenInvalidName_WhenConstructingDecisionVariable_ThenThrowArgumentException(string? invalidName)
    {
        // Act
        Action when = () => _ = new DecisionVariable(invalidName!, ["State1"]);

        // Assert
        when.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("1bad")]
    [InlineData("bad name")]
    [InlineData("bad_")]
    [InlineData("-bad")]
    public void GivenNameWithInvalidPattern_WhenConstructingDecisionVariable_ThenThrowArgumentException(string invalidName)
    {
        // Act
        Action when = () => _ = new DecisionVariable(invalidName, ["State1"]);

        // Assert
        when.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GivenNullStates_WhenConstructingDecisionVariable_ThenThrowArgumentNullException()
    {
        // Act
        Action when = () => _ = new DecisionVariable("Valid", null!);

        // Assert
        when.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GivenEmptyStates_WhenConstructingDecisionVariable_ThenThrowArgumentException()
    {
        // Act
        Action when = () => _ = new DecisionVariable("Valid", []);

        // Assert
        when.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void GivenStatesWithNullOrWhitespace_WhenConstructingDecisionVariable_ThenThrowArgumentException(string? invalidState)
    {
        // Arrange
        List<string?> states = ["State1", invalidState];

        // Act
        Action when = () => _ = new DecisionVariable("Valid", states!);

        // Assert
        when.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("1bad")]
    [InlineData("bad state")]
    [InlineData("bad_")]
    [InlineData("-bad")]
    public void GivenStatesWithInvalidPattern_WhenConstructingDecisionVariable_ThenThrowArgumentException(string invalidState)
    {
        // Arrange
        List<string> states = ["State1", invalidState];

        // Act
        Action when = () => _ = new DecisionVariable("Valid", states);

        // Assert
        when.Should().Throw<ArgumentException>();
    }
}