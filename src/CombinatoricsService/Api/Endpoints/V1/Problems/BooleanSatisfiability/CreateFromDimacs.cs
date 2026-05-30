using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Features.Problems;
using Raijin.CombinatoricsService.Application.Features.Problems.BooleanSatisfiability;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems.BooleanSatisfiability;

public sealed class CreateBooleanSatisfiabilityFromDimacsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapPost("problems/sat-dimacs", Execute)
            .WithName("create sat problem from dimacs")
            .WithTags("sat")
            .Accepts<CreateBooleanSatisfiabilityFromDimacsRequest>("multipart/form-data")
            .Produces<CreateBooleanSatisfiabilityFromDimacsResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .DisableAntiforgery();
    }

    public static async Task<IResult> Execute(
        [FromForm] CreateBooleanSatisfiabilityFromDimacsRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken
    )
    {
        Result<string> dimacsResult = await ReadDimacs(request.File, ".cnf", cancellationToken);

        if (dimacsResult.IsFailed)
            return dimacsResult.ToProblemResult();

        Result<CreateBooleanSatisfiabilityFromDimacsResult> result = await mediator.Send(
            new CreateBooleanSatisfiabilityFromDimacsCommand(
                new ProblemDetailsDto(
                    request.Name,
                    request.Description ?? string.Empty
                ),
                dimacsResult.Value
            ),
            cancellationToken);

        return result.IsSuccess
            ? TypedResults.Created($"/problems/{result.Value.ProblemId}", new CreateBooleanSatisfiabilityFromDimacsResponse(result.Value.ProblemId))
            : result.ToProblemResult();
    }

    private static async Task<Result<string>> ReadDimacs(
        IFormFile? file,
        string expectedExtension,
        CancellationToken cancellationToken)
    {
        if (file is null)
            return Result.Fail(new ValidationError(nameof(CreateBooleanSatisfiabilityFromDimacsRequest.File), "DIMACS file is required."));

        if (!string.Equals(Path.GetExtension(file.FileName), expectedExtension, StringComparison.OrdinalIgnoreCase))
            return Result.Fail(new ValidationError(nameof(CreateBooleanSatisfiabilityFromDimacsRequest.File), $"DIMACS file must have {expectedExtension} extension."));

        await using Stream stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);

        return await reader.ReadToEndAsync(cancellationToken);
    }
}

public sealed class CreateBooleanSatisfiabilityFromDimacsRequest
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public IFormFile File { get; set; } = null!;
}

public sealed record CreateBooleanSatisfiabilityFromDimacsResponse(Guid ProblemId);
