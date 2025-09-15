using Raijin.ProblemSolvingService.Application.Features.Dtos;

namespace Raijin.ProblemSolvingService.Api.Endpoints.V1.Shared.Requests;

public sealed record ClauseRequest(List<LiteralRequest> Literals)
{
    public ClauseDto ToClauseDto() => new(Literals.Select(l => l.ToLiteralDto()).ToList());
}