using FluentAssertions;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Domain.Tests.SatProblems;

[Trait("Category", "Unit")]
public class SatResultTests
{
    [Fact]
    public void GivenAssignments_WhenCreatingSolvableResult_ThenResultIsSolvableWithAssignments()
    {
        var assignments = new List<SatVariableAssignment>
        {
            new(new SatVariable(1), true),
            new(new SatVariable(2), false)
        };

        SatResult result = SatResult.Solvable(assignments);

        result.Status.Should().Be(SolvingStatus.Solvable);
        result.Assignments.Should().BeEquivalentTo(assignments);
    }

    [Fact]
    public void WhenCreatingUnsolvableResult_ThenResultIsUnsolvableAndHasNoAssignments()
    {
        SatResult result = SatResult.Unsolvable();

        result.Status.Should().Be(SolvingStatus.Unsolvable);
        result.Assignments.Should().BeEmpty();
    }

    [Fact]
    public void WhenCreatingIndeterminateResult_ThenResultIsIndeterminateAndHasNoAssignments()
    {
        SatResult result = SatResult.Indeterminate();

        result.Status.Should().Be(SolvingStatus.Indeterminate);
        result.Assignments.Should().BeEmpty();
    }

}