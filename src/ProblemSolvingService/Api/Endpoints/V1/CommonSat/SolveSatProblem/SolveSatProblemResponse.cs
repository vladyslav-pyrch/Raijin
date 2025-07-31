using Raijin.ProblemSolvingService.Api.Endpoints.V1.CommonSat.Shared.Responses;
using Raijin.ProblemSolvingService.Application.Features.CommonSat.Commands.SolveSatProblem;
using Raijin.ProblemSolvingService.Application.Features.CommonSat.Dtos;

namespace Raijin.ProblemSolvingService.Api.Endpoints.V1.CommonSat.SolveSatProblem;

public record SolveSatProblemResponse(
    SolvingStatusResponse SolvingStatus,
    List<VariableAssignmentResponse> VariableAssignments)
{
    public static SolveSatProblemResponse FromSolveSatProblemCommandResult(SolveSatProblemCommandResult commandResult)
    {
        SolvingStatusResponse solvingStatusResponse = commandResult.SolvingStatus switch
        {
            SolvingStatusDto.Satisfiable => SolvingStatusResponse.Satisfiable,
            SolvingStatusDto.Unsatisfiable => SolvingStatusResponse.Unsatisfiable,
            SolvingStatusDto.Indeterminate => SolvingStatusResponse.Indeterminate,
            _ => throw new ArgumentOutOfRangeException(nameof(commandResult.SolvingStatus), commandResult.SolvingStatus, null)
        };

        return new SolveSatProblemResponse(
            solvingStatusResponse,
            commandResult.VariableAssignments.Select(VariableAssignmentResponse.FromVariableAssignmentDto).ToList()
        );
    }
}