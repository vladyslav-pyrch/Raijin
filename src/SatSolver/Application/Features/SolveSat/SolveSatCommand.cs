using Raijin.SatSolver.Application.Cqrs;

namespace Raijin.SatSolver.Application.Features.SolveSat;

public record SolveSatCommand(string Dimacs) : IRequest;