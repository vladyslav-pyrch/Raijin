using System.Text;
using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Domain.Logic;

public sealed record SatReduction
{
    private int? _numberOfVariables;
    
    private int? _numberOfClauses;

    public SatReduction(Guid id, IEnumerable<IEnumerable<int>> clauses, IReadOnlyBijectiveDictionary<Variable, int> symbolTable)
    {
        ArgumentNullException.ThrowIfNull(clauses);
        ArgumentNullException.ThrowIfNull(symbolTable);
        List<IEnumerable<int>> clausesCopy = clauses.ToList();

        if (!clausesCopy.Any())
            throw new ArgumentException("A SAT reduction must contain at least one clause.", nameof(clauses));
        if (clausesCopy.Any(element => element is null))
            throw new ArgumentException("A clause in the clauses is null.", nameof(clauses));

        Id = id;
        Clauses = clausesCopy;
        SymbolTable = symbolTable;
    }

    public Guid Id { get; }

    public IEnumerable<IEnumerable<int>> Clauses { get; }

    public IReadOnlyBijectiveDictionary<Variable, int> SymbolTable { get; }

    public int NumberOfVariables
    {
        get
        {
            _numberOfVariables ??= Clauses.SelectMany(clause => clause).Select(Math.Abs).Distinct().Count();

            return _numberOfVariables.Value;
        }
    }

    public int NumberOfClauses
    {
        get
        {
            _numberOfClauses ??= Clauses.Count();

            return _numberOfClauses.Value;
        }
    }

    public string Dimacs
    {
        get
        {
            if (field is not null)
                return field;
            
            StringBuilder sb = new();
            sb.AppendLine($"p cnf {NumberOfVariables} {NumberOfClauses}");

            foreach (IEnumerable<int> clause in Clauses)
                sb.AppendLine($"{string.Join(" ", clause)} 0");
            
            field = sb.ToString();

            return field;
        }
    } = null;
}