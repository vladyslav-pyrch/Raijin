using Raijin.ProblemSolvingService.Application.Features.Dtos;

namespace Raijin.ProblemSolvingService.Api.Endpoints.V1.Shared.Requests;

public sealed record LiteralRequest(int VariableNumber, bool IsNegated)
{
    public LiteralDto ToLiteralDto() => new(VariableNumber, IsNegated);
}