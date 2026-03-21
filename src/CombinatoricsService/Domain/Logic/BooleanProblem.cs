using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.Logic;

public class BooleanProblem(Guid id)
{
    private readonly Dictionary<string, Variable> _variables = [];

    public Guid Id { get; } = id;

    public string Formula { get; private set; }

    public ExpressionNode Expression { get; private set; }

    public IEnumerable<Variable> Variables => _variables.Values;

    public Satisfiability Satisfiability { get; private set; } = Satisfiability.Unknown;

    public BooleanProblemSolution? Solution { get; private set; }

    public static BooleanProblem Rehydrate(Guid id, string formula)
    {
        ArgumentNullException.ThrowIfNull(formula);

        var problem = new BooleanProblem(id)
        {
            Formula = formula,
            Expression = ExpressionParser.Parse(formula)
        };

        foreach (Variable variable in problem.Expression.GetVariables())
            problem._variables.TryAdd(variable.Name, variable);

        return problem;
    }

    public SatReduction ReduceToSat()
    {
        return TseitinTransform(Id);
    }

    public void SetExpression(ExpressionNode expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        SetExpression(expression, expression.ToString());
    }

    public void SetExpression(string formula)
    {
        ArgumentNullException.ThrowIfNull(formula);

        SetExpression(ExpressionParser.Parse(formula), formula);
    }

    public void ResolveSatSolution(SatSolution satSolution)
    {
        ArgumentNullException.ThrowIfNull(satSolution);

        if (satSolution.NumberOfVariables == 0)
        {
            Satisfiability = Satisfiability.Unsatisfiable;
            return;
        }

        SatReduction satReduction = ReduceToSat();
        if (satSolution.NumberOfVariables != satReduction.NumberOfVariables)
            throw new ArgumentException(
                $"The SAT solution contains {satSolution.NumberOfVariables} variable(s) but the reduction expects {satReduction.NumberOfVariables}.",
                nameof(satSolution));

        IReadOnlyBijectiveDictionary<int, Variable> inverse = satReduction.SymbolTable.Inverse;

        IEnumerable<VariableAssignment> assignments = satSolution.Literals
            .Select(literal => new { Index = Math.Abs(literal), Value = literal > 0 })
            .Where(arg => inverse.ContainsKey(arg.Index))
            .Select(arg => new VariableAssignment(inverse[arg.Index], arg.Value));

        Solution = new BooleanProblemSolution(assignments);
        Satisfiability = Satisfiability.Satisfiable;
    }

    private void SetExpression(ExpressionNode expression, string formula)
    {
        ArgumentNullException.ThrowIfNull(expression);
        ArgumentNullException.ThrowIfNull(formula);

        Formula = formula;
        Expression = expression;

        foreach (Variable variable in Expression.GetVariables())
            _variables.TryAdd(variable.Name, variable);
    }

    private SatReduction TseitinTransform(Guid satReductionId)
    {
        var varId = 1;
        BijectiveDictionary<Variable, int> symbolTable = [];
        List<IEnumerable<int>> clauses = [];

        int lastVariable = Expression.TseitinTransform(clauses, symbolTable, () => varId++);
        clauses.Add([lastVariable]);

        return new SatReduction(satReductionId, clauses, symbolTable);
    }
}