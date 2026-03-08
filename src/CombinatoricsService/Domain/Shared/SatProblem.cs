namespace Raijin.CombinatoricsService.Domain.Shared;

public sealed record SatProblem
{
    public SatProblem(IEnumerable<Clause> clauses)
    {
        ArgumentNullException.ThrowIfNull(clauses);
        List<Clause> clausesCopy = clauses.ToList();

        if (!clausesCopy.Any())
            throw new ArgumentException("A sat problem must contain at least one clause.", nameof(clauses));
        if (clausesCopy.Any(element => element is null))
            throw new ArgumentException("A clause in the clauses is null.", nameof(clauses));

        Clauses = clausesCopy;
    }

    public IEnumerable<Clause> Clauses { get; }

    public int NumberOfVariables => Clauses.SelectMany(clause => clause.Literals)
        .Select(literal => literal.Number)
        .Distinct().Count();

    public int NumberOfClauses => Clauses.Count();
    
    public string ToDimacs()
    {
        var header = $"p cnf {NumberOfVariables} {NumberOfClauses}";
        string body = string.Join(Environment.NewLine, Clauses.Select(clause => clause.ToDimacs()));
        return $"{header}{Environment.NewLine}{body}";
    }
}