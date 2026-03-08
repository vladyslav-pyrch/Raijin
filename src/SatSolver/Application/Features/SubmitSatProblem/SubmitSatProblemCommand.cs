using FluentResults;
using Raijin.SatSolver.Application.Messaging;

namespace Raijin.SatSolver.Application.Features.SubmitSatProblem;

public record SubmitSatProblemCommand(string Dimacs) : IRequest<Result<SubmitSatProblemResult>>;