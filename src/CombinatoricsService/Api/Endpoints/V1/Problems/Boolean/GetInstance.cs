using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Features.Problems.Boolean;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems.Boolean;

public sealed class GetBooleanInstanceEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapGet("problems/{id:Guid}/bool/instance", Execute)
            .WithName("get boolean instance")
            .WithTags("bool");
    }

    public static async Task<Results<
        Ok<GetBooleanInstanceResponse>,
        NotFound<ProblemDetails>,
        ValidationProblem,
        InternalServerError
    >> Execute(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        Result<GetBooleanInstanceResult> result =
            await mediator.Send(new GetBooleanInstanceQuery(id), cancellationToken);

        if (result.IsSuccess)
            return TypedResults.Ok(new GetBooleanInstanceResponse(result.Value.Instance));

        if (result.Has(out IReadOnlyList<ValidationError>? validationErrors))
            return validationErrors.ToValidationProblemResult();

        if (result.Has(out NotFoundError? notFoundError))
            return notFoundError.ToNotFoundResult();

        return TypedResults.InternalServerError();
    }
}

public sealed record GetBooleanInstanceResponse(BooleanProblemInstanceDto Instance);
