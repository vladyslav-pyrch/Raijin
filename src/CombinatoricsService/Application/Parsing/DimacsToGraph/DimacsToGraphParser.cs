using FluentResults;
using Microsoft.Extensions.Logging;
using Raijin.CombinatoricsService.Domain.Graphs;

namespace Raijin.CombinatoricsService.Application.Parsing.DimacsToGraph;

public sealed class DimacsToGraphParser(ILogger<DimacsToGraphParser> logger) : IDimacsToGraphParser
{
    public Result<Graph> Parse(string input)
    {
        List<IError> errors = [];
        List<ParsedEdge> parsedEdges = [];
        Header? header = null;
        List<int> headerLineNumbers = [];
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
                headerLineNumbers.Add(lineNumber);

                if (sawNonCommentLine)
                    errors.Add(new Error($"Line {lineNumber}: Header line must be the first non-comment line. Only comment lines starting with 'c' may appear before it."));

                Result<Header> parsedHeader = ParseHeader(line, lineNumber);
                if (parsedHeader.IsSuccess && header is null)
                    header = parsedHeader.Value;
                else if (parsedHeader.IsFailed)
                    errors.AddRange(parsedHeader.Errors);

                sawNonCommentLine = true;
                continue;
            }

            sawNonCommentLine = true;

            if (line.StartsWith('e'))
            {
                Result<ParsedEdge> parsedEdge = ParseEdge(line, lineNumber);
                
                if (parsedEdge.IsSuccess)
                    parsedEdges.Add(parsedEdge.Value);
                else
                    errors.AddRange(parsedEdge.Errors);

                continue;
            }

            errors.Add(new Error($"Line {lineNumber}: Line is ill formatted. Expected a comment starting with 'c', a header 'p edge <number of vertices> <number of edges>', or an edge 'e <vertex u> <vertex v>'."));
        }

        if (headerLineNumbers.Count == 0)
            errors.Add(new Error("Missing header line. Expected exactly one header line with format 'p edge <number of vertices> <number of edges>'."));
        else if (headerLineNumbers.Count > 1)
            errors.Add(new Error($"There should be exactly one header line, but found {headerLineNumbers.Count} on lines {string.Join(", ", headerLineNumbers)}."));

        if (header is not null)
        {
            if (parsedEdges.Count > header.EdgeCount)
                errors.Add(new Error($"Header on line {header.LineNumber} declares {header.EdgeCount} edges, but {parsedEdges.Count} edge lines were parsed. Remove extra edge lines or update the header."));
            else if (parsedEdges.Count < header.EdgeCount)
                errors.Add(new Error($"Header on line {header.LineNumber} declares {header.EdgeCount} edges, but only {parsedEdges.Count} edge lines were parsed. Add missing edge lines or update the header."));

            foreach (ParsedEdge edge in parsedEdges)
            {
                if (edge.U > header.VertexCount)
                    errors.Add(new Error($"Line {edge.LineNumber}: Edge endpoint {edge.U} is outside the declared vertex range 1..{header.VertexCount}."));

                if (edge.V > header.VertexCount)
                    errors.Add(new Error($"Line {edge.LineNumber}: Edge endpoint {edge.V} is outside the declared vertex range 1..{header.VertexCount}."));
            }
        }

        if (errors.Count > 0)
        {
            logger.LogWarning(
                "DIMACS graph parse failed. InputLength={InputLength} LineCount={LineCount} ErrorCount={ErrorCount}",
                input.Length,
                lines.Length,
                errors.Count);
            return Result.Fail(errors);
        }

        if (header is null)
        {
            logger.LogWarning(
                "DIMACS graph parse failed. InputLength={InputLength} LineCount={LineCount} ErrorCount={ErrorCount}",
                input.Length,
                lines.Length,
                1);
            return Result.Fail(new Error("Missing valid header line. Expected format 'p edge <number of vertices> <number of edges>'."));
        }

        List<Vertex> vertices = CreateVertices(header.VertexCount);

        List<Edge> edges = parsedEdges
            .Select((edge, index) => new Edge($"e{index + 1}", vertices[edge.U - 1], vertices[edge.V - 1]))
            .ToList();

        logger.LogDebug(
            "DIMACS graph parsed. InputLength={InputLength} LineCount={LineCount} VertexCount={VertexCount} EdgeCount={EdgeCount}",
            input.Length,
            lines.Length,
            vertices.Count,
            edges.Count);

        return Result.Ok(new Graph(vertices, edges));
    }

    private static Result<Header> ParseHeader(string line, int lineNumber)
    {
        string[] tokens = SplitLine(line);
        List<IError> errors = [];

        if (tokens.Length != 4)
        {
            errors.Add(new Error($"Line {lineNumber}: Header line is ill formatted. Expected 4 tokens: 'p edge <number of vertices> <number of edges>', but found {tokens.Length}."));
            return Result.Fail(errors);
        }

        if (tokens[0] != "p")
            errors.Add(new Error($"Line {lineNumber}: Header line must start with 'p', but found '{tokens[0]}'."));

        if (tokens[1] != "edge")
            errors.Add(new Error($"Line {lineNumber}: Header problem type must be 'edge', but found '{tokens[1]}'."));

        if (!int.TryParse(tokens[2], out int vertexCount))
            errors.Add(new Error($"Line {lineNumber}: Header vertex count '{tokens[2]}' is not an integer."));
        else if (vertexCount < 0)
            errors.Add(new Error($"Line {lineNumber}: Header vertex count must be non-negative, but found {vertexCount}."));

        if (!int.TryParse(tokens[3], out int edgeCount))
            errors.Add(new Error($"Line {lineNumber}: Header edge count '{tokens[3]}' is not an integer."));
        else if (edgeCount < 0)
            errors.Add(new Error($"Line {lineNumber}: Header edge count must be non-negative, but found {edgeCount}."));

        if (errors.Count > 0)
            return Result.Fail(errors);

        return Result.Ok(new Header(vertexCount, edgeCount, lineNumber));
    }

    private static Result<ParsedEdge> ParseEdge(string line, int lineNumber)
    {
        string[] tokens = SplitLine(line);
        List<IError> errors = [];

        if (tokens.Length != 3)
        {
            errors.Add(new Error($"Line {lineNumber}: Edge line is ill formatted. Expected 3 tokens: 'e <vertex u> <vertex v>', but found {tokens.Length}."));
            return Result.Fail(errors);
        }

        if (tokens[0] != "e")
            errors.Add(new Error($"Line {lineNumber}: Edge line must start with 'e', but found '{tokens[0]}'."));

        if (!int.TryParse(tokens[1], out int u))
            errors.Add(new Error($"Line {lineNumber}: First edge endpoint '{tokens[1]}' is not an integer."));
        else if (u <= 0)
            errors.Add(new Error($"Line {lineNumber}: First edge endpoint must be a positive integer, but found {u}."));

        if (!int.TryParse(tokens[2], out int v))
            errors.Add(new Error($"Line {lineNumber}: Second edge endpoint '{tokens[2]}' is not an integer."));
        else if (v <= 0)
            errors.Add(new Error($"Line {lineNumber}: Second edge endpoint must be a positive integer, but found {v}."));

        if (errors.Count > 0)
            return Result.Fail(errors);

        return Result.Ok(new ParsedEdge(u, v, lineNumber));
    }

    // Null separator makes string.Split use all whitespace characters, including tabs.
    // Cast selects the char[] overload explicitly.
    private static string[] SplitLine(string line) =>
        line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

    private static List<Vertex> CreateVertices(int vertexCount) =>
        Enumerable
            .Range(1, vertexCount)
            .Select(index => new Vertex($"v{index}"))
            .ToList();

    private sealed record Header(int VertexCount, int EdgeCount, int LineNumber);

    private sealed record ParsedEdge(int U, int V, int LineNumber);
}
