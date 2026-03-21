namespace Raijin.CombinatoricsService.Domain.CombinatoricProblems;

public sealed record DecisionVariableAssignment
{
    public DecisionVariableAssignment(DecisionVariable decisionVariable, string selectedState)
    {
        ArgumentNullException.ThrowIfNull(decisionVariable);
        ArgumentException.ThrowIfNullOrWhiteSpace(selectedState);

        if (!decisionVariable.States.Contains(selectedState))
            throw new ArgumentException(
                $"The state '{selectedState}' is not a valid state of decision variable '{decisionVariable.Name}'. " +
                $"Valid states: [{string.Join(", ", decisionVariable.States)}].",
                nameof(selectedState));

        DecisionVariable = decisionVariable;
        SelectedState = selectedState;
    }

    public DecisionVariable DecisionVariable { get; }

    public string SelectedState { get; }
}

