using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace Raijin.SatSolver.Domain.SatProblems;

public partial class SatProblem
{
    private const string DimacsHeaderPrefix = "p cnf";

    private SatProblem(Guid id, int numberOfVariables, int numberOfClauses, IEnumerable<int[]> clauses)
    {
        Id = id;
        NumberOfVariables = numberOfVariables;
        NumberOfClauses = numberOfClauses;
        Clauses = clauses;
    }

    public Guid Id { get; }

    private int NumberOfVariables { get; }

    private int NumberOfClauses { get; }

    private IEnumerable<int[]> Clauses { get; }

    [field: AllowNull, MaybeNull]
    public string Dimacs
    {
        get
        {
            field ??= GenerateDimacs();
            return field;
        }
        private set;
    }

    public Satisfiability Satisfiability { get; private set; } = Satisfiability.Unknown;

    public int[] Solution
    {
        get => Satisfiability != Satisfiability.Unknown
            ? field
            : throw new InvalidOperationException("Solution is not available yet.");
        private set;
    } = [];

    public void SetSolution(int[] solution)
    {
        ValidateSolution(solution);
        Solution = solution;
        Satisfiability = solution.Length == 0 ? Satisfiability.Unsatisfiable : Satisfiability.Satisfiable;
    }

    private void ValidateSolution(int[] solution)
    {
        if (solution is [])
            return;

        if (solution.Any(literal => Math.Abs(literal) > NumberOfVariables))
            throw new ArgumentException(
                $"Invalid solution: Literal with index greater than number of variables ({NumberOfVariables}) found.");

        if (solution.Select(Math.Abs).Distinct().Count() != NumberOfVariables)
            throw new ArgumentException(
                $"Invalid solution: Solution must assign a value to all variables from 1 to {NumberOfVariables}.");
    }

    private string GenerateDimacs()
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"{DimacsHeaderPrefix} {NumberOfVariables} {NumberOfClauses}");

        foreach (int[] clause in Clauses)
            stringBuilder.AppendLine($"{string.Join(" ", clause)} 0");

        return stringBuilder.ToString();
    }

    public static SatProblem Create(Guid id, string dimacs)
    {
        string[] lines = dimacs.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

        string headerLine = lines.FirstOrDefault(line => line.StartsWith(DimacsHeaderPrefix))
                            ?? throw new FormatException("Invalid DIMACS format: Missing header line.");

        if (!DimacsHeaderLineFormatRegex().IsMatch(headerLine))
            throw new FormatException(
                "Invalid DIMACS format: Header line must be in format `p cnf number_of_variables number_of_clauses`.");

        string[] headerParts = headerLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int numberOfVariables = int.Parse(headerParts[2]);
        int numberOfClauses = int.Parse(headerParts[3]);

        string[] clauseLines = lines.Where(line => !line.StartsWith('c') && !line.StartsWith('p')).ToArray();

        if (clauseLines.Length == 0)
            throw new FormatException("Invalid DIMACS format: No clause lines found.");

        if (clauseLines.Any(line => !line.EndsWith(" 0")))
            throw new FormatException("Invalid DIMACS format: Each clause line must end with ' 0'.");

        List<int[]> clauses =  clauseLines.Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(part => part != "0")
            .Select(int.Parse)
            .ToArray()
        ).ToList();

        if (clauses.Count != numberOfClauses)
            throw new FormatException($"Invalid DIMACS format: Number of clauses specified in header ({numberOfClauses}) does not match actual number of clauses ({clauses.Count}).");

        int maxVariableIndex = clauses.SelectMany(clause => clause).Select(Math.Abs).Max();
        if (maxVariableIndex > numberOfVariables)
            throw new FormatException(
                $"Invalid DIMACS format: Number of variables specified in header ({numberOfVariables}) is less than the maximum variable index used in clauses ({maxVariableIndex}).");

        return new SatProblem(id, numberOfVariables, numberOfClauses, clauses)
        {
            Dimacs = dimacs // Store original DIMACS for reference
        };
    }

    [GeneratedRegex(@"^p cnf \d+ \d+$")]
    private static partial Regex DimacsHeaderLineFormatRegex();
}