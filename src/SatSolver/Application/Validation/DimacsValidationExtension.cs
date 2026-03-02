using FluentValidation;

namespace Raijin.SatSolver.Application.Validation;

public static class DimacsValidationExtension
{
    public static IRuleBuilderOptionsConditions<T, string> MustBeDimacs<T>(this IRuleBuilder<T, string> ruleBuilder)
        => ruleBuilder.Custom(ValidateDimacs);

    private static void ValidateDimacs<T>(string dimacs, ValidationContext<T> context)
    {
        if (dimacs is null)
            return;

        List<(string Line, int Index)> lines = ParseLines(dimacs);

        if (lines.Count == 0)
        {
            context.AddFailure("Missing problem line (p cnf <vars> <clauses>).");
            return;
        }

        (string Line, int Index) problem = lines[0];
        if (!TryParseProblemLine(problem, context, out int numVars, out int numClauses))
            return;

        List<(string Line, int Index)> clauseLines = lines.Skip(1).ToList();
        if (!ValidateClauseCount(clauseLines, numClauses, context))
            return;

        ValidateClauses(clauseLines, numVars, context);
    }


    private static List<(string Line, int Index)> ParseLines(string dimacs) => dimacs
        .Split('\n', StringSplitOptions.RemoveEmptyEntries)
        .Select((line, index) => (Line: line.Trim(), Inded: index + 1))
        .Where(l => !l.Line.StartsWith('c'))
        .ToList();

    private static bool TryParseProblemLine<T>(
        (string Line, int Index) problem,
        ValidationContext<T> context,
        out int numVars,
        out int numClauses)
    {
        numVars = 0;
        numClauses = 0;

        string[] parts = problem.Line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 4 || parts[0] != "p" || parts[1] != "cnf")
        {
            context.AddFailure(
                $"Line {problem.Index}: Invalid problem line. Expected: p cnf <vars> <clauses>."
            );
            return false;
        }

        bool validNumberOfVars = int.TryParse(parts[2], out numVars) && numVars > 0;
        bool validNumberOfClauses = int.TryParse(parts[3], out numClauses) && numClauses > 0;

        if (!validNumberOfVars)
            context.AddFailure($"Line {problem.Index}: Invalid variable count '{parts[2]}'.");

        if (!validNumberOfClauses)
            context.AddFailure($"Line {problem.Index}: Invalid clause count '{parts[3]}'.");

        return validNumberOfVars && validNumberOfClauses;
    }

    private static bool ValidateClauseCount<T>(
        List<(string Line, int Index)> clauseLines,
        int expected,
        ValidationContext<T> context)
    {
        if (clauseLines.Count == expected)
            return true;

        context.AddFailure($"Expected {expected} clause lines but found {clauseLines.Count}.");
        return false;
    }

    private static void ValidateClauses<T>(
        List<(string Line, int Index)> clauseLines,
        int numVars,
        ValidationContext<T> context)
    {
        foreach ((string line, int index) in clauseLines)
        {
            string[] tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length == 0)
            {
                context.AddFailure($"Line {index}: Clause is empty.");
                continue;
            }

            if (tokens[^1] != "0")
                context.AddFailure($"Line {index}: Clause must end with 0.");

            ValidateClauseLiterals(tokens, index, numVars, context);
        }
    }

    private static void ValidateClauseLiterals<T>(
        string[] tokens,
        int lineNumber,
        int numVars,
        ValidationContext<T> context)
    {
        for (var i = 0; i < tokens.Length - 1; i++)
        {
            string token = tokens[i];

            if (!int.TryParse(token, out int literal))
            {
                context.AddFailure($"Line {lineNumber}: '{token}' is not a valid integer literal.");
                continue;
            }

            if (literal == 0)
                context.AddFailure($"Line {lineNumber}: Literal 0 is only allowed as clause terminator.");

            if (Math.Abs(literal) > numVars)
                context.AddFailure($"Line {lineNumber}: Literal {literal} exceeds declared variable count {numVars}.");
        }
    }
}