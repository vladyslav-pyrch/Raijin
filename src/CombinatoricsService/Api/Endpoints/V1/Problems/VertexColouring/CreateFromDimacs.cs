using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Features.Problems;
using Raijin.CombinatoricsService.Application.Features.Problems.VertexColoring;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Api.Endpoints.V1.Problems.VertexColouring;

public sealed class CreateVertexColoringFromDimacsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapPost("problems/vertex-coloring-dimacs", Execute)
            .WithName("create vertex coloring problem from dimacs")
            .WithTags("vertex-coloring")
            .Accepts<CreateVertexColoringFromDimacsRequest>("multipart/form-data")
            .Produces<CreateVertexColoringFromDimacsResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .DisableAntiforgery();
    }

    public static async Task<IResult> Execute(
        [FromForm] CreateVertexColoringFromDimacsRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken
    )
    {
        Result<string> dimacsResult = await ReadDimacs(request.File, ".col", cancellationToken);

        if (dimacsResult.IsFailed)
            return dimacsResult.ToProblemResult();

        Result<CreateVertexColoringFromDimacsResult> result = await mediator.Send(
            new CreateVertexColoringFromDimacsCommand(
                new ProblemDetailsDto(
                    request.Name,
                    request.Description ?? string.Empty
                ),
                dimacsResult.Value,
                request.ColorCount
            ),
            cancellationToken);

        return result.IsSuccess
            ? TypedResults.Created($"/problems/{result.Value.ProblemId}", new CreateVertexColoringFromDimacsResponse(result.Value.ProblemId))
            : result.ToProblemResult();
    }

    private static async Task<Result<string>> ReadDimacs(
        IFormFile? file,
        string expectedExtension,
        CancellationToken cancellationToken)
    {
        if (file is null)
            return Result.Fail(new ValidationError(nameof(CreateVertexColoringFromDimacsRequest.File), "DIMACS file is required."));

        if (!string.Equals(Path.GetExtension(file.FileName), expectedExtension, StringComparison.OrdinalIgnoreCase))
            return Result.Fail(new ValidationError(nameof(CreateVertexColoringFromDimacsRequest.File), $"DIMACS file must have {expectedExtension} extension."));

        await using Stream stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);

        return await reader.ReadToEndAsync(cancellationToken);
    }
}

public sealed class CreateVertexColoringFromDimacsRequest
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int ColorCount { get; set; }

    public IFormFile File { get; set; } = null!;
}

public sealed record CreateVertexColoringFromDimacsResponse(Guid ProblemId);
