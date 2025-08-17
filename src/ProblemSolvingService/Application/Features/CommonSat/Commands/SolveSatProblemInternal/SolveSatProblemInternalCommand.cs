using Raijin.ProblemSolvingService.Application.Cqrs;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Application.Features.CommonSat.Commands.SolveSatProblemInternal;

public record SolveSatProblemInternalCommand(SatProblem SatProblem) : IRequest<SatResult>;