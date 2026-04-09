using System.Text;

namespace Raijin.CombinatoricsService.Domain.Shared;

public sealed record SatEncoding
{
    private SatEncoding(IEnumerable<IEnumerable<int>> clauses, VariableMap variableMap)
    {
        Clauses = clauses;
        VariableMap = variableMap;
    }

    public IEnumerable<IEnumerable<int>> Clauses { get; }

    public int NumberOfVariables => Clauses.Select(ints => ints.Max()).Append(0).Max();

    public int NumberOfClauses => Clauses.Count();

    public VariableMap VariableMap { get; }

    public static SatEncoding Create(IEnumerable<IEnumerable<int>> clauses, VariableMap variableMap)
    {
        ArgumentNullException.ThrowIfNull(clauses);
        ArgumentNullException.ThrowIfNull(variableMap);

        return new SatEncoding(clauses, variableMap);
    }

    public static SatEncoding Rehydrate(IEnumerable<IEnumerable<int>> clauses, VariableMap variableMap) =>
        new(clauses, variableMap);

    public string ToDimacs()
    {
        var sb = new StringBuilder();

        sb.AppendLine($"p cnf {NumberOfVariables} {NumberOfClauses}");

        foreach (IEnumerable<int> clause in Clauses)
        {
            foreach (int i in clause)
                sb.Append(i).Append(' ');
            sb.Append(0).AppendLine();
        }

        return sb.ToString();
    }
}