using Raijin.ProblemSolvingService.Application.Features.CommonSat.Dtos;

namespace Raijin.ProblemSolvingService.Api.Endpoints.V1.CommonSat.Shared.Requests;

public record ClauseRequest(List<LiteralRequest> Literals)
{
    public ClauseDto ToClauseDto() => new(Literals.Select(l => l.ToLiteralDto()).ToList());
}