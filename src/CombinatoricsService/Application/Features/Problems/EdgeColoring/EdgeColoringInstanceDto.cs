using FluentValidation;
using Raijin.CombinatoricsService.Application.Features.Problems.Graphs;

namespace Raijin.CombinatoricsService.Application.Features.Problems.EdgeColoring;

public sealed record EdgeColoringInstanceDto(
    GraphDto Graph,
    int ColorCount
);

public sealed class EdgeColoringInstanceDtoValidator : AbstractValidator<EdgeColoringInstanceDto>
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