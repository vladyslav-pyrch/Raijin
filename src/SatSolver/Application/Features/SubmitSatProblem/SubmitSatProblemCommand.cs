using Raijin.SatSolver.Application.Messaging;

namespace Raijin.SatSolver.Application.Features.SubmitSatProblem;

public sealed record SubmitSatProblemCommand(string Dimacs, Guid? SatProblemId = null)
    : IRequest<SubmitSatProblemResult>;