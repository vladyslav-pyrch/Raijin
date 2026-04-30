using FluentValidation;
using Raijin.CombinatoricsService.Application.Features.Problems.Graphs;

namespace Raijin.CombinatoricsService.Application.Features.Problems.VertexColoring;

public sealed record VertexColoringInstanceDto(
    GraphDto Graph,
    int ColorCount
);

public sealed class EdgeColoringInstanceDtoValidator : AbstractValidator<VertexColoringInstanceDto>
{
    public EdgeColoringInstanceDtoValidator(IValidator<GraphDto> graphValidator)
    {
        RuleFor(dto => dto.Graph)
            .NotNull()
            .SetValidator(graphValidator);
        RuleFor(dto => dto.ColorCount)
            .GreaterThan(0)
            .WithMessage("Color count must be greater than 0.");
    }
}