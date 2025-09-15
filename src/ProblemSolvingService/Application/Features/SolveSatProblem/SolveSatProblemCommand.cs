using Raijin.ProblemSolvingService.Application.Cqrs;
using Raijin.ProblemSolvingService.Application.Features.Dtos;

namespace Raijin.ProblemSolvingService.Application.Features.SolveSatProblem;

public sealed record SolveSatProblemCommand(List<ClauseDto> Clauses) : IRequest<SolveSatProblemCommandResult>;