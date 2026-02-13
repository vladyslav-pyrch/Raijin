using FluentAssertions;
using Raijin.SatSolver.Domain.SatProblems;

namespace Raijin.SatSolver.Domain.Tests.SatProblems;

public class SatProblemTests
{
    [Fact]
    public void GivenDimacs_WhenCreate_ThenSatProblemIsCreated()
    {
        // Given
        var dimacs = "p cnf 3 2\n1 -3 0\n-1 2 3 0";

        // When
        var satProblem = SatProblem.Create(dimacs);

        // Then
        satProblem.Should().NotBeNull();
    }

    [Fact]
    public void GivenProblemWithUnknownSatisfiability_WhenGetSolution_ThenThrowInvalidOperationException()
    {
        // Given
        var dimacs = "p cnf 3 2\n1 -3 0\n-1 2 3 0";
        var satProblem = SatProblem.Create(dimacs);

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
        var satProblem = SatProblem.Create(dimacs);
        var solution = Array.Empty<int>();

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
        var satProblem = SatProblem.Create(dimacs);
        var solution = new[] { 1, -2, 3 };

        // When
        satProblem.SetSolution(solution);

        // Then
        satProblem.Satisfiability.Should().Be(Satisfiability.Satisfiable);
    }
}