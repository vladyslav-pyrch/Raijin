using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Features.Problems;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems;

public sealed class GetSatEncodingVariableMapEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapGet("problems/{id:Guid}/sat-encoding/variable-map", Execute)
            .WithName("get sat encoding variable map")
            .WithTags("problems", "sat-encoding");
    }

    public static async Task<Results<
        Ok<GetSatEncodingVariableMapResponse>,
        NotFound<ProblemDetails>,
        ValidationProblem,
        InternalServerError
    >> Execute(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        Result<GetSatEncodingVariableMapResult> result = await mediator.Send(new GetSatEncodingVariableMapQuery(id), cancellationToken);

        if (result.IsSuccess)
            return TypedResults.Ok(new GetSatEncodingVariableMapResponse(
                result.Value.Variables
                    .Select(v => new VariableMapEntryResponse(v.Name, v.Index))
                    .ToList()));

        if (result.Has(out IReadOnlyList<ValidationError>? validationErrors))
            return validationErrors.ToValidationProblemResult();

        if (result.Has(out NotFoundError? notFoundError))
            return notFoundError.ToNotFoundResult();

        return TypedResults.InternalServerError();
    }
}

public sealed record GetSatEncodingVariableMapResponse(IReadOnlyList<VariableMapEntryResponse> Variables);

public sealed record VariableMapEntryResponse(string Name, int Index);
