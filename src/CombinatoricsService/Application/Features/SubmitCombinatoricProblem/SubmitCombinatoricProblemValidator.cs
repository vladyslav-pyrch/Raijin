using System.Text.RegularExpressions;
using FluentValidation;
using Raijin.CombinatoricsService.Domain.Logic;

namespace Raijin.CombinatoricsService.Application.Features.SubmitCombinatoricProblem;

public sealed partial class SubmitCombinatoricProblemValidator : AbstractValidator<SubmitCombinatoricProblemCommand>
{
    public SubmitCombinatoricProblemValidator()
    {
        RuleFor(command => command.DecisionVariables)
            .NotEmpty()
            .WithMessage("The decision variables must not be empty")
            .Must(variables => variables.Select(v => v.Name).Distinct().Count() == variables.Length)
            .WithMessage("Each decision variable must have a unique name");

        RuleFor(command => command.Constraints)
            .NotNull()
            .WithMessage("The constraints must not be null");

        RuleForEach(command => command.DecisionVariables)
            .ChildRules(decisionVariable =>
            {
                decisionVariable.RuleFor(variable => variable.Name)
                    .NotEmpty()
                    .WithMessage("The name of a decision variable must not be empty")
                    .Matches("^[a-zA-Z][a-zA-Z0-9-]*$")
                    .WithMessage(
                        "The name of a decision variable must start with a letter and can only contain letters, digits, and hyphens");

                decisionVariable.RuleFor(variable => variable.States)
                    .NotEmpty()
                    .WithMessage("A decision variable must have at least two states")
                    .Must(states => states.Length >= 2)
                    .WithMessage("A decision variable must have at least two states")
                    .Must(states => states.Distinct().Count() == states.Length)
                    .WithMessage("Each state must have a unique name within a decision variable");

                decisionVariable.RuleForEach(variable => variable.States)
                    .NotEmpty()
                    .WithMessage("The name of a state must not be empty")
                    .Matches("^[a-zA-Z][a-zA-Z0-9-]*$")
                    .WithMessage(
                        "The name of a state must start with a letter and can only contain letters, digits, and hyphens");
            });

        RuleForEach(command => command.Constraints)
            .NotEmpty()
            .WithMessage("A constraint must not be empty")
            .Custom((constraint, context) =>
            {
                try
                {
                    ExpressionNode expression = ExpressionParser.Parse(constraint);

                    IEnumerable<Variable> variablesInExpression = expression.GetVariables();

                    Dictionary<string, string[]> decisionVariables =
                        context.InstanceToValidate.DecisionVariables.ToDictionary(variable => variable.Name,
                            variable => variable.States);

                    foreach (Variable variable in variablesInExpression)
                    {
                        string[] parts = variable.Name.Split("_is_");

                        if (parts.Length != 2)
                        {
                            context.AddFailure(
                                $"The variable '{variable.Name}' in the constraint '{constraint}' does not follow the required format 'DecisionVariableName_is_StateName'");
                            continue;
                        }

                        string decisionVariableName = parts[0];
                        string decisionVariableState = parts[1];

                        if (!ValidNamePattern().IsMatch(decisionVariableName))
                            context.AddFailure(
                                $"The decision variable name '{decisionVariableName}' of variable '{variable.Name}' in the constraint '{constraint}' does not match the required pattern '{ValidNamePattern()}'");

                        if (!ValidNamePattern().IsMatch(decisionVariableState))
                            context.AddFailure(
                                $"The decision variable state '{decisionVariableState}' of variable '{variable.Name}' in the constraint '{constraint}' does not match the required pattern '{ValidNamePattern()}'");

                        if (!decisionVariables.ContainsKey(decisionVariableName))
                            context.AddFailure(
                                $"The decision variable '{decisionVariableName}' used in the constraint '{constraint}' is not defined in the decision variables");

                        if (decisionVariables.TryGetValue(decisionVariableName, out string[]? states) &&
                            !states.Contains(decisionVariableState))
                            context.AddFailure(
                                $"The state '{decisionVariableState}' of decision variable '{decisionVariableName}' used in the constraint '{constraint}' is not defined in the decision variables");
                    }
                }
                catch (Exception ex)
                {
                    context.AddFailure($"The constraint '{constraint}' is not a valid expression: {ex.Message}");
                }
            });
    }

    [GeneratedRegex("[a-zA-Z][a-zA-Z0-9-]*")]
    private static partial Regex ValidNamePattern();
}