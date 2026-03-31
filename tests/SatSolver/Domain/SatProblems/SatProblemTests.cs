using FluentAssertions;
using Raijin.SatSolver.Domain.SatProblems;

namespace Raijin.SatSolver.Domain.Tests.SatProblems;

[Trait("Category", "Unit")]
[Trait("Service", "SatSolver")]
public class SatProblemTests
{
    [Fact]
    public void GivenDimacs_WhenCreate_ThenSatProblemIsCreated()
    {
        // Given
        var dimacs = "p cnf 3 2\n1 -3 0\n-1 2 3 0";

        // When
        var satProblem = SatProblem.Create(Guid.CreateVersion7(), dimacs);

        // Then
        satProblem.Should().NotBeNull();
    }

    [Fact]
    public void GivenProblemWithUnknownSatisfiability_WhenGetSolution_ThenThrowInvalidOperationException()
    {
        // Given
        var dimacs = "p cnf 3 2\n1 -3 0\n-1 2 3 0";
        var satProblem = SatProblem.Create(Guid.CreateVersion7(), dimacs);

        // When
        Func<int[]> getSolution = () => satProblem.Solution;

        // Then
        getSolution.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GivenEmptySolution_WhenSetSolution_ThenSatisfiabilityIsUnsatisfiable()
    {
        // Given
        var dimacs = "p cnf 3 2\n1 -3 0\n-1 2 3 0";
        var satProblem = SatProblem.Create(Guid.CreateVersion7(), dimacs);
        int[] solution = [];

        // When
        satProblem.SetSolution(solution);

        // Then
        satProblem.Satisfiability.Should().Be(Satisfiability.Unsatisfiable);
    }

    [Fact]
    public void GivenNonEmptySolution_WhenSetSolution_ThenSatisfiabilityIsSatisfiable()
    {
        // Given
        var dimacs = "p cnf 3 2\n1 -3 0\n-1 2 3 0";
        var satProblem = SatProblem.Create(Guid.CreateVersion7(), dimacs);
        int[] solution = [1, -2, 3];

        // When
        satProblem.SetSolution(solution);

        // Then
        satProblem.Satisfiability.Should().Be(Satisfiability.Satisfiable);
    }

    [Fact]
    public void GivenSolutionThatDoesNotSetAllTheVariables_WhenSetSolution_ThenThrowArgumentException()
    {
        // Given
        var dimacs = "p cnf 3 2\n1 -3 0\n-1 2 3 0";
        var satProblem = SatProblem.Create(Guid.CreateVersion7(), dimacs);
        int[] solution = [1, -2];

        // When
        Action setSolution = () => satProblem.SetSolution(solution);

        // Then
        setSolution.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GivenSolutionThatHasVariablesWithValuesOutOfRange_WhenSetSolution_ThenThrowArgumentException()
    {
        // Given
        var dimacs = "p cnf 3 2\n1 -3 0\n-1 2 3 0";
        var satProblem = SatProblem.Create(Guid.CreateVersion7(), dimacs);
        int[] solution = [1, -4, 3];

        // When
        Action setSolution = () => satProblem.SetSolution(solution);

        // Then
        setSolution.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GivenSatisfiableProblem_WhenGetSolution_ThenReturnSolution()
    {
        // Given
        var dimacs = "p cnf 3 2\n1 -3 0\n-1 2 3 0";
        var satProblem = SatProblem.Create(Guid.CreateVersion7(), dimacs);
        int[] solution = [1, -2, 3];
        satProblem.SetSolution(solution);

        // When
        int[] result = satProblem.Solution;

        // Then
        result.Should().Equal(solution);
    }

    [Fact]
    public void GivenInvalidDimacs_WhenCreate_ThenThrowFormatException()
    {
        // Given
        var invalidDimacs = "invalid dimacs format";

        // When
        Action createSatProblem = () => SatProblem.Create(Guid.CreateVersion7(), invalidDimacs);

        // Then
        createSatProblem.Should().Throw<FormatException>();
    }

    [Fact]
    public void GivenInvalidHeaderDimacs_WhenCreate_ThenThrowFormatException()
    {
        // Given
        var invalidHeaderDimacs = "p cnf 3\n1 -3 0\n-1 2 3 0";

        // When
        Action createSatProblem = () => SatProblem.Create(Guid.CreateVersion7(), invalidHeaderDimacs);

        // Then
        createSatProblem.Should().Throw<FormatException>();
    }

    [Fact]
    public void GivenEmptyHeaderDimacs_WhenCreate_ThenThrowFormatException()
    {
        // Given
        var emptyHeaderDimacs = "p cnf 0 0\n";

        // When
        Action createSatProblem = () => SatProblem.Create(Guid.CreateVersion7(), emptyHeaderDimacs);

        // Then
        createSatProblem.Should().Throw<FormatException>();
    }

    [Fact]
    public void GivenClauseWithInvalidFormat_WhenCreate_ThenThrowFormatException()
    {
        // Given
        var invalidClauseDimacs = "p cnf 3 2\n1 -3\n-1 2 3 0";

        // When
        Action createSatProblem = () => SatProblem.Create(Guid.CreateVersion7(), invalidClauseDimacs);

        // Then
        createSatProblem.Should().Throw<FormatException>();
    }

    [Fact]
    public void GivenEmptyClause_WhenCreate_ThenThrowFormatException()
    {
        // Given
        var emptyClauseDimacs = "p cnf 3 2\n0\n-1 2 3 0";

        // When
        Action createSatProblem = () => SatProblem.Create(Guid.CreateVersion7(), emptyClauseDimacs);

        // Then
        createSatProblem.Should().Throw<FormatException>();
    }

    [Fact]
    public void GivenNumberOfClausesMismatchDimacs_WhenCreate_ThenThrowFormatException()
    {
        // Given
        var numberOfClausesMismatchDimacs = "p cnf 3 2\n1 -3 0\n-1 2 3 0\n1 2 0";

        // When
        Action createSatProblem = () => SatProblem.Create(Guid.CreateVersion7(), numberOfClausesMismatchDimacs);

        // Then
        createSatProblem.Should().Throw<FormatException>();
    }

    [Fact]
    public void GivenNumberOfVariablesMismatchDimacs_WhenCreate_ThenThrowFormatException()
    {
        // Given
        var numberOfVariablesMismatchDimacs = "p cnf 2 2\n1 -3 0\n-1 2 3 0";

        // When
        Action createSatProblem = () => SatProblem.Create(Guid.CreateVersion7(), numberOfVariablesMismatchDimacs);

        // Then
        createSatProblem.Should().Throw<FormatException>();
    }
}