using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Features.Problems.ConstraintSatisfiability;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems.ConstraintSatisfiability;

public sealed class GetCspInstanceEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapGet("problems/{id:Guid}/instance/csp", Execute)
            .WithName("get csp instance")
            .WithTags("problems", "csp");
    }

    public static async Task<Results<Ok<GetCspInstanceResponse>, NotFound<ProblemDetails>, ValidationProblem, InternalServerError>> Execute(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        Result<GetCspInstanceResult> result =
            await mediator.Send(new GetCspInstanceQuery(id), cancellationToken);

        if (result.IsSuccess)
        {
            var value = result.Value;
            return TypedResults.Ok(new GetCspInstanceResponse(
                value.Variables.Select(v => new CspVariableResponse(v.Name, v.States)).ToList(),
                value.Constraints));
        }

        if (result.Has(out IReadOnlyList<ValidationError>? validationErrors))
            return validationErrors.ToValidationProblemResult();

        if (result.Has(out NotFoundError? notFoundError))
            return notFoundError.ToNotFoundResult();

        return TypedResults.InternalServerError();
    }
}

public sealed record GetCspInstanceResponse(
    IReadOnlyList<CspVariableResponse> Variables,
    IReadOnlyList<string> Constraints
);

public sealed record CspVariableResponse(string Name, IReadOnlyList<string> States);
