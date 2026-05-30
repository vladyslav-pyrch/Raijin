using FluentResults;
using Raijin.CombinatoricsService.Domain.Graphs;

namespace Raijin.CombinatoricsService.Application.Parsing.DimacsToGraph;

public interface IDimacsToGraphParser
{
    public Result<Graph> Parse(string input);
}