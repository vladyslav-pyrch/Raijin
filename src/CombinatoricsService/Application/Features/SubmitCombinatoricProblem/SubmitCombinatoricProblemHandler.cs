using FluentResults;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.CombinatoricProblems;

namespace Raijin.CombinatoricsService.Application.Features.SubmitCombinatoricProblem;

public sealed class SubmitCombinatoricProblemHandler(
    ICombinatoricProblemRepository combinatoricProblemRepository,
    IUnitOfWork unitOfWork,
    IMessageBus messageBus,
    ILogger<SubmitCombinatoricProblemHandler> logger
) : IRequestHandler<SubmitCombinatoricProblemCommand, SubmitCombinatoricProblemResult>
{
    public async Task<Result<SubmitCombinatoricProblemResult>> Handle(
        SubmitCombinatoricProblemCommand request,
        CancellationToken cancellationToken
    )
    {
        var combinatoricProblemId = Guid.CreateVersion7();

        CombinatoricProblem combinatoricProblem = new(combinatoricProblemId);

        foreach (DecisionVariableDto variableDto in request.DecisionVariables)
            combinatoricProblem.AddDecisionVariable(variableDto.Name, variableDto.States);

        Result result = new();
        for (var i = 0; i < request.Constraints.Length; i++)
        {
            int i1 = i;
            Result addingConstraintResult = Result.Try(
                () => combinatoricProblem.AddConstrain(request.Constraints[i1]),
                exception => new ValidationError(
                    $"{nameof(request.Constraints)}[{i1}]",
                    exception.Message
                )
            );

            result.WithErrors(addingConstraintResult.Errors);
        }

        if (result.IsFailed)
            return result;

        await combinatoricProblemRepository.Add(combinatoricProblem, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        await messageBus.Publish<ICombinatoricProblemSubmitted>(new
        {
            CombinatoricProblemId = combinatoricProblemId,
            DecisionVariables =
                combinatoricProblem.DecisionVariables.Select(variable => new { variable.Name, variable.States })
                    .ToArray(),
            Constraints = combinatoricProblem.Constraints.Select(constraint => constraint.Formula).ToArray()
        }, cancellationToken);

        return new SubmitCombinatoricProblemResult(combinatoricProblemId);
    }
}