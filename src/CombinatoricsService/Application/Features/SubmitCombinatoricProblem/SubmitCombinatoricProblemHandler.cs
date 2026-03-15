using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Validation;
using Raijin.CombinatoricsService.Domain.CombinatoricProblems;
using Raijin.CombinatoricsService.Domain.Logic;

namespace Raijin.CombinatoricsService.Application.Features.SubmitCombinatoricProblem;

public class SubmitCombinatoricProblemHandler(
    IValidator<SubmitCombinatoricProblemCommand> validator,
    IMessageBus messageBus,
    ILogger<SubmitCombinatoricProblemHandler> logger
) : ICommandHandler<SubmitCombinatoricProblemCommand, Result<SubmitCombinatoricProblemResult>>
{
    public async Task<Result<SubmitCombinatoricProblemResult>> Handle(
        SubmitCombinatoricProblemCommand command,
        CancellationToken cancellationToken
    )
    {
        logger.LogInformation("Handling {CommandName} with {DecisionVariableCount} decision variables and {ConstraintCount} constraints",
            nameof(SubmitCombinatoricProblemCommand), command.DecisionVariables.Length, command.Constraints.Length);
        
        var result = new Result();

        ValidationResult? validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            result.WithErrors(validationResult.ToValidationErrors());

        var parsedConstraints = new List<ExpressionNode>(command.Constraints.Length);
        Dictionary<string, HashSet<string>> decisionVariableToStates = command.DecisionVariables.ToDictionary(
            variable => variable.Name,
            variable => variable.States.ToHashSet()
        );

        for (var i = 0; i < command.Constraints.Length; i++)
        {
            Result<ExpressionNode> parsingResult = ConstrainParser.ParseExpression(command.Constraints[i])
                .MapErrors(error => new ValidationError(
                    propertyName: $"{nameof(SubmitCombinatoricProblemCommand.Constraints)}[{i}]",
                    problem: error.Message
                ));

            if (parsingResult.IsFailed)
            {
                result.WithErrors(parsingResult.Errors);
                continue;
            }

            //Delegate that validation to the parser
            foreach (StateNode stateNode in parsingResult.Value.Leaves().OfType<StateNode>())
            {
                if (!decisionVariableToStates.TryGetValue(stateNode.DecisionVariableName, out HashSet<string>? states))
                    parsingResult.WithError(new ValidationError(
                        propertyName: $"{nameof(SubmitCombinatoricProblemCommand.Constraints)}[{i}]",
                        problem:
                        $"The variable '{stateNode.DecisionVariableName}' is used in the constraints but is not defined in the decision variables"
                    ));

                if (states is not null && !states.Contains(stateNode.DecisionVariableState))
                    parsingResult.WithError(new ValidationError(
                        propertyName: $"{nameof(SubmitCombinatoricProblemCommand.Constraints)}[{i}]",
                        problem:
                        $"The state '{stateNode.DecisionVariableState}' of variable '{stateNode.DecisionVariableName}' is used in the constraints but is not defined in the decision variables"
                    ));
            }

            if (parsingResult.IsFailed)
            {
                result.WithErrors(parsingResult.Errors);
                continue;
            }

            parsedConstraints.Add(parsingResult.Value);
        }
        
        if (result.IsFailed)
            return result;

        var combinatoricProblemId = Guid.NewGuid();
        var combinatoricProblem = new CombinatoricProblem(combinatoricProblemId);

        foreach (DecisionVariableDto variableDto in command.DecisionVariables)
            combinatoricProblem.AddDecisionVariable(variableDto.Name, variableDto.States);
        foreach (ExpressionNode constraint in parsedConstraints)
            combinatoricProblem.AddConstrain(constraint);

        //TODO Save the problem to the database

        TseitinTransformResult transformResult = combinatoricProblem.ToFormula().TseitinTransform();
        


        await messageBus.Publish<ISatProblemSubmitted>(new
        {
            SatProblemId = combinatoricProblemId,
            Dimacs = transformResult.Problem.ToDimacs()
        }, cancellationToken);

        return new SubmitCombinatoricProblemResult(combinatoricProblemId);
    }
}