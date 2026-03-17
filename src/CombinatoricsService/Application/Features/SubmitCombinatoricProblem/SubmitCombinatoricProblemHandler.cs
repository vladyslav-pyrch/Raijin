using FluentResults;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.CombinatoricProblems;
using Raijin.CombinatoricsService.Domain.Logic;

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
        var combinatoricProblemId = Guid.NewGuid();
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
            return result;

        TseitinTransformResult transformResult = combinatoricProblem.ToFormula().TseitinTransform();
        
        await combinatoricProblemRepository.Add(combinatoricProblem, cancellationToken);

        await messageBus.Publish<ICombinatoricProblemSubmitted>(new
        {
            CombinatoricProblemId = combinatoricProblemId,
            DecisionVariables = combinatoricProblem.DecisionVariables.Select(variable => new
            {
                Name = variable.Name,
                States = variable.States
            }).ToArray(),
            Constraints = combinatoricProblem.Constraints.Select(constraint => constraint.Formula).ToArray()
        }, cancellationToken);
        
        await messageBus.Publish<ISatProblemSubmitted>(new
        {
            SatProblemId = combinatoricProblemId,
            Dimacs = transformResult.Problem.ToDimacs()
        }, cancellationToken);
        
        await unitOfWork.SaveChanges(cancellationToken);

        return new SubmitCombinatoricProblemResult(combinatoricProblemId);
    }
}