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
            [FromServices] IMessageIdGenerator messageIdGenerator,
            [FromServices] ILogger<SubmitCombinatoricProblemEndpoint> logger,
            CancellationToken cancellationToken
        )
    {
        logger.LogInformation("Received SubmitCombinatoricProblem request with {VariableCount} decision variables and {ConstraintCount} constraints",
            request.DecisionVariables.Length, request.Constraints.Length);

        Result<SubmitCombinatoricProblemResult> result = await mediator.Send(new SubmitCombinatoricProblemCommand(
            request.DecisionVariables.Select(dv => new DecisionVariableDto(dv.Name, dv.States)).ToArray(),
            request.Constraints,
            messageIdGenerator.NextMessageContext()
        ), cancellationToken);

        if (result.IsSuccess)
        {
            logger.LogInformation("SubmitCombinatoricProblem succeeded, CombinatoricProblemId: {CombinatoricProblemId}", result.Value.CombinatoricsProblemId);
            return TypedResults.Ok(new SubmitCombinatoricProblemResponse
            {
                CombinatoricsProblemId = result.Value.CombinatoricsProblemId
            });
        }

        if (result.HasError<ValidationError>())
        {
            logger.LogWarning("SubmitCombinatoricProblem validation failed: {Errors}", string.Join("; ", result.Errors.Select(e => e.Message)));
            return TypedResults.ValidationProblem(result.ToValidationErrorDictionary());
        }

        logger.LogError("SubmitCombinatoricProblem failed with unexpected error: {Errors}", string.Join("; ", result.Errors.Select(e => e.Message)));
        return TypedResults.InternalServerError();
    }
}