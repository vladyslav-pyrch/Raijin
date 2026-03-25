using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Application.Features.ResolveBooleanProblem;

public sealed record ResolveBooleanProblemCommand(Guid BooleanProblemId, SatSolutionDto SatSolution) : IRequest;