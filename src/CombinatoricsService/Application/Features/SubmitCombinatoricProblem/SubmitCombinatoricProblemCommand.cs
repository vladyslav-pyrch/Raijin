using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Application.Features.SubmitCombinatoricProblem;

public sealed record SubmitCombinatoricProblemCommand(
    DecisionVariableDto[] DecisionVariables,
    string[] Constraints
) : IRequest<SubmitCombinatoricProblemResult>;