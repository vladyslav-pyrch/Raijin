using FluentResults;
using Raijin.SatSolver.Application.Cqrs;

namespace Raijin.SatSolver.Application.Features.SubmitSatProblem;

public record SubmitSatProblemCommand(string Dimacs) : IRequest<Result<SubmitSatProblemResult>>;