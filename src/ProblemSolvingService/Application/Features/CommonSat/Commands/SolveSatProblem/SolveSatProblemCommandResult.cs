using Raijin.ProblemSolvingService.Application.Features.CommonSat.Dtos;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Application.Features.CommonSat.Commands.SolveSatProblem;

public record SolveSatProblemCommandResult(
    SolvingStatusDto SolvingStatus,
    List<VariableAssignmentDto> VariableAssignments)
{
    public static SolveSatProblemCommandResult FromSatResult(SatResult satResult)
    {
        // Map result back to DTOs
        SolvingStatusDto solvingStatus = satResult.Status switch
        {
            Domain.SatProblems.SolvingStatus.Solvable => SolvingStatusDto.Satisfiable,
            Domain.SatProblems.SolvingStatus.Unsolvable => SolvingStatusDto.Unsatisfiable,
            Domain.SatProblems.SolvingStatus.Indeterminate => SolvingStatusDto.Indeterminate,
            _ => throw new ArgumentOutOfRangeException()
        };

        List<VariableAssignmentDto> variableAssignments = satResult.Assignments
            .Select(assignment => new VariableAssignmentDto(assignment.SatVariable.Id, assignment.Value))
            .ToList();

        return new SolveSatProblemCommandResult(solvingStatus, variableAssignments);
    }
}