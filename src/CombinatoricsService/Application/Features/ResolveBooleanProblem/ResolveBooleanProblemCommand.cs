using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Application.Features.ResolveBooleanProblem;

public record ResolveBooleanProblemCommand(Guid BooleanProblemId, int[] SatSolution) : IRequest;