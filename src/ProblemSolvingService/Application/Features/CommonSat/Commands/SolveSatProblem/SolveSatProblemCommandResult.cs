using Raijin.ProblemSolvingService.Application.Features.CommonSat.Dtos;

namespace Raijin.ProblemSolvingService.Application.Features.CommonSat.Commands.SolveSatProblem;

public record SolveSatProblemCommandResult(SolvingStatusDto SolvingStatus, List<VariableAssignmentDto> VariableAssignments);