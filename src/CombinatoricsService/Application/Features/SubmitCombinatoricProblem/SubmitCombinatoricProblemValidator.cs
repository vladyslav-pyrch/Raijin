using FluentValidation;

namespace Raijin.CombinatoricsService.Application.Features.SubmitCombinatoricProblem;

public sealed class SubmitCombinatoricProblemValidator : AbstractValidator<SubmitCombinatoricProblemCommand>
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
                    .WithMessage("The name of a decision variable must start with a letter and can only contain letters, digits, and hyphens");
                
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
                    .WithMessage("The name of a state must start with a letter and can only contain letters, digits, and hyphens");
            });
    }
}