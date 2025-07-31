using Raijin.ProblemSolvingService.Application.Features.CommonSat.Dtos;

namespace Raijin.ProblemSolvingService.Api.Endpoints.V1.CommonSat.Shared.Requests;

public record LiteralRequest(int VariableNumber, bool IsNegated)
{
    public LiteralDto ToLiteralDto() => new(VariableNumber, IsNegated);
}