using Raijin.SatSolver.Application.Messaging;

namespace Raijin.SatSolver.Application.Features.SubmitSatProblem;

public sealed record SubmitSatProblemCommand(string Dimacs, MessageContext Context) 
    : IRequest<SubmitSatProblemResult>, IContextualRequest;