using Raijin.ProblemSolvingService.Application.Cqrs;
using Raijin.ProblemSolvingService.Domain.SatProblems;

namespace Raijin.ProblemSolvingService.Application.Features.CommonSat.Commands.SolveSatProblem;

public record SolveSatProblemInternalCommand(SatProblem SatProblem) : ICommand<SatResult>;