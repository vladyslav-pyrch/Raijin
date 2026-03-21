using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.Logic;

public class BooleanProblem
{
    private readonly Dictionary<string, Variable> _variables = [];
    
    public BooleanProblem(Guid id, string formula)
    {
        Id = id;
        Formula = formula;
        Expression = ExpressionParser.Parse(formula);
        
        foreach (Variable variable in Expression.GetVariables())
            _variables.Add(variable.Name, variable);
    }
    
    public BooleanProblem(Guid id, ExpressionNode expression)
    {
        Id = id;
        Formula = expression.ToString();
        Expression = expression;
        
        foreach (Variable variable in Expression.GetVariables())
            _variables.TryAdd(variable.Name, variable);
    } 
    
    public Guid Id { get; }
    
    public string Formula { get; }
    
    public ExpressionNode Expression { get; }
    
    public IEnumerable<Variable> Variables => _variables.Values;

    public Satisfiability Satisfiability { get; private set; } = Satisfiability.Unknown;

    public Assignment? Solution { get; private set; }
    
    public SatReduction GetSatReduction() => TseitinTransform(Id);

    public void SetSolution(IReadOnlyDictionary<string, bool> solution)
    {
        ArgumentNullException.ThrowIfNull(solution);

        if (solution.Count == 0)
        {
            Satisfiability = Satisfiability.Unsatisfiable;
            return;
        }
        
        string[] variableNames = Variables.Select(variable => variable.Name ).ToArray();
        
        if (solution.Keys.Except(variableNames).Any())
            throw new ArgumentException("The solution contains variables that are not defined in the problem.", nameof(solution));
        if (variableNames.Except(solution.Keys).Any())
            throw new ArgumentException("The solution does not contain all variables defined in the problem.", nameof(solution));

        IEnumerable<VariableAssignment> variableAssignments = solution
            .Select(kvp => new VariableAssignment(_variables[kvp.Key], kvp.Value));
        
        Solution = new Assignment(variableAssignments);
        Satisfiability = Satisfiability.Satisfiable;
    }

    private SatReduction TseitinTransform(Guid satReductionId)
    {
        var varId = 1;
        BijectiveDictionary<Variable, int> symbolTable = [];
        List<IEnumerable<int>> clauses = [];
        
        int lastVariable = Expression.TseitinTransform(clauses, symbolTable, newLiteralId: () => varId++);
        clauses.Add([lastVariable]);

        return new SatReduction(satReductionId, clauses, symbolTable);
    }
}