using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Application.Features.ResolveCombinatoricProblem;

public sealed record ResolveCombinatoricProblemCommand(
    Guid CombinatoricProblemId,
    BooleanProblemSolutionDto BooleanProblemSolutionSolution
) : IRequest;