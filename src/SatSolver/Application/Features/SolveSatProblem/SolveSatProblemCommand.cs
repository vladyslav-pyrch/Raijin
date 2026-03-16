using FluentResults;
using Raijin.SatSolver.Application.Messaging;

namespace Raijin.SatSolver.Application.Features.SolveSatProblem;

public record SolveSatProblemCommand(Guid SatProblemId, string Dimacs) : ICommand<Result>;