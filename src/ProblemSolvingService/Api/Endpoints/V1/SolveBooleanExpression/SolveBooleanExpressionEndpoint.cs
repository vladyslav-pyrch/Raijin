using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.ProblemSolvingService.Application.Cqrs;
using Raijin.ProblemSolvingService.Application.Features.SolveBooleanExpression;

namespace Raijin.ProblemSolvingService.Api.Endpoints.V1.SolveBooleanExpression;

public static class SolveBooleanExpressionEndpoint
{
    public static async Task<Results<Ok<SolveBooleanExpressionResponse>, ValidationProblem>> Execute(
        [FromBody] SolveBooleanExpressionRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken = default)
    {
        Result<SolveBooleanExpressionResult> result =
            await sender.Send(request.ToSolveBooleanExpressionCommand(), cancellationToken);

        if (result.IsSuccess)
            return TypedResults.Ok(SolveBooleanExpressionResponse.From(result.Value));

        Dictionary<string, string[]> errors = [];
        string message = result.Errors.OfType<BooleanExpressionParseError>().First().Message;
        errors.Add(nameof(request.Expression), [message]);
        return TypedResults.ValidationProblem(errors);
    }
}