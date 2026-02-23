using Raijin.SatSolver.Application.Cqrs;

namespace Raijin.SatSolver.Application.Features.SolveSatProblem;

public record SolveSatProblemCommand(Guid SatProblemId, string Dimacs) : IRequest;