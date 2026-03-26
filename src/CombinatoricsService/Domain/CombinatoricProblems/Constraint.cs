using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Raijin.CombinatoricsService.Domain.Logic;

namespace Raijin.CombinatoricsService.Domain.CombinatoricProblems;

public partial record Constraint
{
    public Constraint(string formula)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(formula);

        Formula = formula;

        Expression = ExpressionParser.Parse(formula);

        if (!Expression.GetVariables().All(variable => ValidStateNodePattern().IsMatch(variable.Name)))
            throw new ArgumentException(
                $"All variables in the constraint must match the pattern {ValidStateNodePattern()}",
                nameof(formula)
            );
    }

    public Constraint(ExpressionNode expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        Formula = expression.ToString();

        Expression = expression;

        if (!expression.GetVariables().All(variable => ValidStateNodePattern().IsMatch(variable.Name)))
            throw new ArgumentException(
                $"All variables in the constraint must match the pattern {ValidStateNodePattern()}",
                nameof(expression)
            );
    }

    public string Formula { get; }

    [JsonIgnore] public ExpressionNode Expression { get; }

    [GeneratedRegex("[a-zA-Z][a-zA-Z0-9-]*_is_[a-zA-Z][a-zA-Z0-9-]*")]
    private static partial Regex ValidStateNodePattern();
}