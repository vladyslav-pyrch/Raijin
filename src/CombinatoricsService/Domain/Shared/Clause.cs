namespace Raijin.CombinatoricsService.Domain.Shared;

public sealed record Clause
{
    public Clause(IEnumerable<Literal> literals)
    {
        ArgumentNullException.ThrowIfNull(literals);
        List<Literal> literalsCopy = literals.ToList();

        if (!literalsCopy.Any())
            throw new ArgumentException("A clause must contain at least one literal.", nameof(literals));
        if (literalsCopy.Any(element => element is null))
            throw new ArgumentException("A literal in the literals is null.", nameof(literals));

        Literals = literalsCopy;
    }

    public IEnumerable<Literal> Literals { get; }
    
    public string ToDimacs() => string.Join(" ", Literals.Select(literal => literal.ToDimacs())) + " 0";
}