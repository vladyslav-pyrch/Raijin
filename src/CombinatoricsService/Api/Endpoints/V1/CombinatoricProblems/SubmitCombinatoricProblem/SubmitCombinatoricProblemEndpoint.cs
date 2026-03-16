using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Features.SubmitCombinatoricProblem;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.CombinatoricProblems.SubmitCombinatoricProblem;

public class SubmitCombinatoricProblemEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapPost("/combinatoric-problems", Execute)
            .WithName("SubmitCombinatoricProblem")
            .WithTags("combinatoric-problems");
    }

    public static async Task<Results<Ok<SubmitCombinatoricProblemResponse>, ValidationProblem, InternalServerError>>
        Execute(
            [FromBody] SubmitCombinatoricProblemRequest request,
            [FromServices] IMediator mediator,
            CancellationToken cancellationToken
        )
    {
        Result<SubmitCombinatoricProblemResult> result = await mediator.Send(new SubmitCombinatoricProblemCommand(
            request.DecisionVariables.Select(dv => new DecisionVariableDto(dv.Name, dv.States)).ToArray(),
            request.Constraints
        ), cancellationToken);

        if (result.IsSuccess)
            return TypedResults.Ok(new SubmitCombinatoricProblemResponse
            {
                CombinatoricsProblemId = result.Value.CombinatoricsProblemId
            });

        if (result.HasError<ValidationError>())
            return TypedResults.ValidationProblem(result.ToValidationErrorDictionary());

        return TypedResults.InternalServerError();
    }
}