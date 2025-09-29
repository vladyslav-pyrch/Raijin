using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Application.Features.Dtos;

public enum SolvingStatusDto
{
    Satisfiable,
    Unsatisfiable,
    Indeterminate
}

public static class SolvingStatusDtoHelpers
{
    public static SolvingStatusDto ToDto(this SolvingStatus status) =>
        status switch
        {
            SolvingStatus.Solvable => SolvingStatusDto.Satisfiable,
            SolvingStatus.Unsolvable => SolvingStatusDto.Unsatisfiable,
            SolvingStatus.Indeterminate => SolvingStatusDto.Indeterminate,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
}