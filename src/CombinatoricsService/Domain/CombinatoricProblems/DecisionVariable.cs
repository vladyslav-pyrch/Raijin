using System.Text.RegularExpressions;
using Raijin.CombinatoricsService.Domain.Logic;

namespace Raijin.CombinatoricsService.Domain.CombinatoricProblems;

public sealed partial record DecisionVariable
{
    public DecisionVariable(string name, IEnumerable<string> states)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (!ValidNamePattern().IsMatch(name))
            throw new ArgumentException(
                "The name must start with a letter and can only contain letters, digits, and hyphens.", nameof(name));

        ArgumentNullException.ThrowIfNull(states);
        
        IReadOnlyList<string> statesCopy = [..states];

        if (statesCopy.Count <= 1)
            throw new ArgumentException("A decision variable must have at least two states.", nameof(states));

        if (statesCopy.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("A state of a decision variable may not be null or whitespace", nameof(states));

        if (statesCopy.Any(state => !ValidStatePattern().IsMatch(state)))
            throw new ArgumentException(
                "Each state must start with a letter and can only contain letters, digits, and hyphens.",
                nameof(states));

        Name = name;
        States = statesCopy;
    }

    public string Name { get; }

    public IReadOnlyList<string> States { get; }
    
    public Variable[] ToVariables() => States.Select(state => new Variable($"{Name}_is_{state}")).ToArray();

    [GeneratedRegex("^[a-zA-Z][a-zA-Z0-9-]*$")]
    private partial Regex ValidNamePattern();


    [GeneratedRegex("^[a-zA-Z][a-zA-Z0-9-]*$")]
    private partial Regex ValidStatePattern();
}