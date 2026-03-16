using FluentResults;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Application.Features.SubmitCombinatoricProblem;

public record SubmitCombinatoricProblemCommand(DecisionVariableDto[] DecisionVariables, string[] Constraints) 
    : ICommand<Result<SubmitCombinatoricProblemResult>>;