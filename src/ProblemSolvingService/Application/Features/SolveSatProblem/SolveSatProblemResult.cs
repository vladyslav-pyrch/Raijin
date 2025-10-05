using Raijin.ProblemSolvingService.Application.Features.Dtos;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Application.Features.SolveSatProblem;

public sealed record SolveSatProblemResult(
    SolvingStatusDto SolvingStatus,
    List<SatVariableAssignmentDto> VariableAssignments)
{
    public static SolveSatProblemResult FromSatResult(SatResult satResult)
    {
        // Map result back to DTOs
        SolvingStatusDto solvingStatus = satResult.Status switch
        {
            Domain.SatProblems.SolvingStatus.Solvable => SolvingStatusDto.Solvable,
            Domain.SatProblems.SolvingStatus.Unsolvable => SolvingStatusDto.Unsolvable,
            Domain.SatProblems.SolvingStatus.Indeterminate => SolvingStatusDto.Indeterminate,
            _ => throw new ArgumentOutOfRangeException()
        };

        List<SatVariableAssignmentDto> variableAssignments = satResult.Assignments
            .Select(assignment => new SatVariableAssignmentDto(assignment.SatVariable.Id, assignment.Assignment))
            .ToList();

        return new SolveSatProblemResult(solvingStatus, variableAssignments);
    }
}