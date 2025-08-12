using FluentAssertions;
using Raijin.ProblemSolvingService.Application.Features.CommonSat;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Infrastructure.Tests.Integration;

public abstract class SatSolverTests
{
    protected abstract ISatSolver GetSatSolver(bool withZeroTimeout = false);

    [Fact]
    public async Task GivenProblemIsSolvable_WhenSolving_ThenReturnsSatResultWithSolvableStatusAndSolution()
    {
        ISatSolver satSolver = GetSatSolver();

        var satProblem = new SatProblem();
        satProblem.AddClause(new Clause(literals: [Literal.FromInteger(-1)]));
        satProblem.AddClause(new Clause(literals: [Literal.FromInteger(2)]));

        SatResult satResult = await satSolver.Solve(satProblem);

        satResult.Status.Should().Be(SolvingStatus.Solvable);
        satResult.Assignments.Should()
            .BeEquivalentTo([VariableAssignment.FromInteger(-1), VariableAssignment.FromInteger(2)]);
    }

    [Fact]
    public async Task GivenProblemIsUnsolvable_WhenSolving_ThenReturnsSatResultWithUnsolvableStatusAndEmptySolution()
    {
        ISatSolver satSolver = GetSatSolver();

        var satProblem = new SatProblem();
        satProblem.AddClause(new Clause(literals: [Literal.FromInteger(-1)]));
        satProblem.AddClause(new Clause(literals: [Literal.FromInteger(1)]));

        SatResult satResult = await satSolver.Solve(satProblem);

        satResult.Status.Should().Be(SolvingStatus.Unsolvable);
        satResult.Assignments.Should().BeEquivalentTo(Array.Empty<VariableAssignment>());
    }

    [Fact]
    public async Task GivenTimeout_WhenSolving_ThenReturnsSatResultWithIndeterminateStatusAndEmptySolution()
    {
        ISatSolver satSolver = GetSatSolver(withZeroTimeout:true);

        var satProblem = new SatProblem();
        satProblem.AddClause(new Clause(literals: [Literal.FromInteger(-1)]));
        satProblem.AddClause(new Clause(literals: [Literal.FromInteger(-1)]));

        SatResult satResult = await satSolver.Solve(satProblem);

        satResult.Status.Should().Be(SolvingStatus.Indeterminate);
        satResult.Assignments.Should().BeEquivalentTo(Array.Empty<VariableAssignment>());
    }
}