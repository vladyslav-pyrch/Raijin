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
    IMessageIdGenerator messageIdGenerator,
    IMessageContextAccessor messageContextAccessor,
    ILogger<SubmitCombinatoricProblemHandler> logger
) : IRequestHandler<SubmitCombinatoricProblemCommand, SubmitCombinatoricProblemResult>
{
    public async Task<Result<SubmitCombinatoricProblemResult>> Handle(
        SubmitCombinatoricProblemCommand request,
        CancellationToken cancellationToken
    )
    {
        var combinatoricProblemId = Guid.CreateVersion7();
        logger.LogInformation("Submitting combinatoric problem {CombinatoricProblemId} with {VariableCount} decision variables and {ConstraintCount} constraints",
            combinatoricProblemId, request.DecisionVariables.Length, request.Constraints.Length);

        var combinatoricProblem = new CombinatoricProblem(combinatoricProblemId);

        foreach (DecisionVariableDto variableDto in request.DecisionVariables)
            combinatoricProblem.AddDecisionVariable(variableDto.Name, variableDto.States);
        
        var result = new Result();
        for (var i = 0; i < request.Constraints.Length; i++)
        {
            Result addingConstraintResult = Result.Try(
                () => combinatoricProblem.AddConstrain(request.Constraints[i]),
                exception => new ValidationError(
                    propertyName: $"{nameof(request.Constraints)}[{i}]",
                    problem: exception.Message
                )
            );
        
            result.WithErrors(addingConstraintResult.Errors);
        }
        if (result.IsFailed)
        {
            logger.LogWarning("Combinatoric problem {CombinatoricProblemId} has invalid constraints: {ErrorCount} error(s)",
                combinatoricProblemId, result.Errors.Count);
            return result;
        }
        
        await combinatoricProblemRepository.Add(combinatoricProblem, cancellationToken);

        await messageBus.Publish<ICombinatoricProblemSubmitted>(new
        {
            MessageId = messageIdGenerator.NextMessageId(),
            CorrelationId = messageContextAccessor.CurrentContext.CorrelationId,
            CausationId = messageContextAccessor.CurrentContext.CausationId,
            Timestamp = DateTimeOffset.UtcNow,
            CombinatoricProblemId = combinatoricProblemId,
            DecisionVariables = combinatoricProblem.DecisionVariables.Select(variable => new
            {
                Name = variable.Name,
                States = variable.States
            }).ToArray(),
            Constraints = combinatoricProblem.Constraints.Select(constraint => constraint.Formula).ToArray()
        }, cancellationToken);
        
        await unitOfWork.SaveChanges(cancellationToken);

        logger.LogInformation("Combinatoric problem {CombinatoricProblemId} submitted successfully", combinatoricProblemId);
        return new SubmitCombinatoricProblemResult(combinatoricProblemId);
    }
}