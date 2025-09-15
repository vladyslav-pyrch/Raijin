using Raijin.ProblemSolvingService.Application.Cqrs;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Application.Features.SolveSatProblemInternal;

public sealed record SolveSatProblemInternalCommand(SatProblem SatProblem) : IRequest<SatResult>;