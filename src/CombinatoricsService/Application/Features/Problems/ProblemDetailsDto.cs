using FluentValidation;

namespace Raijin.CombinatoricsService.Application.Features.Problems;

public sealed record ProblemDetailsDto(
    string Name,
    string Description
);

public sealed class ProblemDetailsDtoValidator : AbstractValidator<ProblemDetailsDto>
{
    public ProblemDetailsDtoValidator()
    {
        RuleFor(dto => dto.Name)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(dto => dto.Description)
            .NotNull()
            .MaximumLength(5000);
    }
}