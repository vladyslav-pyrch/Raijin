using FluentResults;
using Microsoft.Extensions.Logging;
using Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;

namespace Raijin.CombinatoricsService.Application.Parsing.DimacsToSat;

public class DimacsToSatParser(ILogger<DimacsToSatParser> logger) : IDimacsToSatParser
{
    public Result<BooleanSatisfiabilityInstance> Parse(string input)
    {
        Result<(List<ParsedClause> parsedClauses, Header header)> parsedClausesResult = ParseDetails(input);
        
        if (parsedClausesResult.IsFailed)
        {
            logger.LogWarning(
                "DIMACS SAT parse failed. InputLength={InputLength} ErrorCount={ErrorCount}",
                input.Length,
                parsedClausesResult.Errors.Count);
            return Result.Fail(parsedClausesResult.Errors);
        }
        List<ParsedClause> parsedClauses = parsedClausesResult.Value.parsedClauses;
        Header header = parsedClausesResult.Value.header;

        Dictionary<int, SatVariable> variables = Enumerable
            .Range(1, header.VariableCount)
            .ToDictionary(index => index, index => new SatVariable($"x{index}"));

        List<Clause> clauses = parsedClauses
            .Select(clause => new Clause(clause.Literals
                .Select(literal => new Literal(variables[Math.Abs(literal)], literal < 0))
                .ToList()))
            .ToList();

        logger.LogDebug(
            "DIMACS SAT parsed. InputLength={InputLength} VariableCount={VariableCount} ClauseCount={ClauseCount}",
            input.Length,
            header.VariableCount,
            clauses.Count);

        return Result.Ok(new BooleanSatisfiabilityInstance(clauses));
    }

    public IEnumerable<IEnumerable<int>> ParseForSatEncoding(string input)
    {
        Result<(List<ParsedClause> parsedClauses, Header header)> parsedClausesResult = ParseDetails(input);
        
        if (parsedClausesResult.IsFailed)
        {
            logger.LogWarning(
                "Stored SAT encoding parse failed. InputLength={InputLength} ErrorCount={ErrorCount}",
                input.Length,
                parsedClausesResult.Errors.Count);
            throw new Exception($"Parsing failed: {parsedClausesResult.Errors.First().Message}");
        }
        
        List<ParsedClause> parsedClauses = parsedClausesResult.Value.parsedClauses;

        logger.LogDebug(
            "Stored SAT encoding parsed. InputLength={InputLength} ClauseCount={ClauseCount}",
            input.Length,
            parsedClauses.Count);

        return parsedClauses.Select(parsedClause => parsedClause.Literals).ToArray();
    }

    private Result<(List<ParsedClause> parsedClauses, Header header)> ParseDetails(string input)
    {
        List<IError> errors = [];
        List<ParsedClause> parsedClauses = [];
        Header? header = null;
        var headerLineCount = 0;
        var sawNonCommentLine = false;

        string[] lines = input.Split(["\r\n", "\n"], StringSplitOptions.None);

        for (var i = 0; i < lines.Length; i++)
        {
            int lineNumber = i + 1;
            string line = lines[i].Trim();

            if (line.Length == 0)
                continue;

            if (line.StartsWith('c'))
                continue;

            if (line.StartsWith('p'))
            {
                headerLineCount++;

                if (sawNonCommentLine)
                    errors.Add(new Error(
                        $"Line {lineNumber}: Header line must be the first non-comment line. Found header after clauses or another non-comment line."));

                Result<Header> parsedHeader = ParseHeader(line, lineNumber);
                if (parsedHeader.IsSuccess && header is null)
                    header = parsedHeader.Value;
                else if (parsedHeader.IsFailed)
                    errors.AddRange(parsedHeader.Errors);

                sawNonCommentLine = true;
                continue;
            }

            sawNonCommentLine = true;

            Result<ParsedClause> parsedClause = ParseClause(line, lineNumber);

            if (parsedClause.IsSuccess)
                parsedClauses.Add(parsedClause.Value);
            else
                errors.AddRange(parsedClause.Errors);
        }

        if (headerLineCount != 1)
            errors.Add(new Error(
                $"There should be exactly one header line in format 'p cnf <number of variables> <number of clauses>'. Found {headerLineCount}."));

        if (header is not null)
        {
            if (parsedClauses.Count > header.ClauseCount)
                errors.Add(new Error(
                    $"There are more clauses than stated in the header. Header declares {header.ClauseCount}, but parsed {parsedClauses.Count}."));
            else if (parsedClauses.Count < header.ClauseCount)
                errors.Add(new Error(
                    $"There are less clauses than stated in the header. Header declares {header.ClauseCount}, but parsed {parsedClauses.Count}."));

            foreach (ParsedClause clause in parsedClauses)
            foreach (int literal in clause.Literals)
            {
                int variableIndex = Math.Abs(literal);

                if (variableIndex > header.VariableCount)
                    errors.Add(new Error(
                        $"Line {clause.LineNumber}: Clause refers to variable {variableIndex}, but header allows only variable indexes from 1 to {header.VariableCount}."));
            }
        }

        if (errors.Count > 0)
            return Result.Fail(errors);

        if (header is null)
            return Result.Fail(new Error(
                "There should be exactly one header line in format 'p cnf <number of variables> <number of clauses>'. Found 0."));

        return (parsedClauses, header);
    }

    private static Result<Header> ParseHeader(string line, int lineNumber)
    {
        var tokens = SplitLine(line);

        if (tokens.Length != 4)
            return Result.Fail(new Error(
                $"Line {lineNumber}: Header line must have exactly 4 tokens: 'p cnf <number of variables> <number of clauses>'. Found {tokens.Length} tokens."));

        if (tokens[0] != "p" || tokens[1] != "cnf")
            return Result.Fail(new Error(
                $"Line {lineNumber}: Header line must start with 'p cnf'. Found '{tokens[0]} {tokens[1]}'."));

        if (!int.TryParse(tokens[2], out int variableCount))
            return Result.Fail(new Error(
                $"Line {lineNumber}: Header variable count must be a non-negative integer. Found '{tokens[2]}'."));

        if (variableCount < 0)
            return Result.Fail(new Error(
                $"Line {lineNumber}: Header variable count must be non-negative. Found {variableCount}."));

        if (!int.TryParse(tokens[3], out var clauseCount))
            return Result.Fail(new Error(
                $"Line {lineNumber}: Header clause count must be a non-negative integer. Found '{tokens[3]}'."));

        if (clauseCount < 0)
            return Result.Fail(new Error(
                $"Line {lineNumber}: Header clause count must be non-negative. Found {clauseCount}."));

        return Result.Ok(new Header(variableCount, clauseCount));
    }

    private static Result<ParsedClause> ParseClause(string line, int lineNumber)
    {
        string[] tokens = SplitLine(line);

        List<int> literals = [];

        for (var i = 0; i < tokens.Length; i++)
        {
            if (!int.TryParse(tokens[i], out int value))
                return Result.Fail(new Error(
                    $"Line {lineNumber}: Clause token '{tokens[i]}' is not an integer. Clause lines must contain integers and end with 0."));

            if (value == int.MinValue)
                return Result.Fail(new Error(
                    $"Line {lineNumber}: Clause token '{tokens[i]}' is outside supported literal range."));

            switch (value)
            {
                case 0 when !IsLastToken():
                    return Result.Fail(new Error(
                        $"Line {lineNumber}: Clause terminator 0 must be the last token. Found additional token '{tokens[i + 1]}'."));
                case 0:
                    return Result.Ok(new ParsedClause(literals, lineNumber));
                default:
                    literals.Add(value);
                    break;
            }

            continue;

            bool IsLastToken() => i == tokens.Length - 1;
        }

        return Result.Fail(new Error(
            $"Line {lineNumber}: Clause line must end with 0."));
    }

    // Null separator makes string.Split use all whitespace characters, including tabs.
    // Cast selects the char[] overload explicitly.
    private static string[] SplitLine(string line) =>
        line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

    private sealed record Header(int VariableCount, int ClauseCount);

    private sealed record ParsedClause(IReadOnlyList<int> Literals, int LineNumber);
}
