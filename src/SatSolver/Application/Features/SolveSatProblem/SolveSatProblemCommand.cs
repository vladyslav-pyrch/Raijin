using Raijin.SatSolver.Application.Messaging;

namespace Raijin.SatSolver.Application.Features.SolveSatProblem;

public sealed record SolveSatProblemCommand(Guid SatProblemId, string Dimacs, MessageContext Context) 
    : IRequest, IContextualRequest;