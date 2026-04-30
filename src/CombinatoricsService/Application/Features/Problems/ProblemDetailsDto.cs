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
            .NotEmpty().WithMessage("Problem name is required.")
            .MaximumLength(100).WithMessage("Problem name must not exceed 100 characters.");
        RuleFor(dto => dto.Description)
            .NotNull().WithMessage("Problem description is required.")
            .MaximumLength(5000).WithMessage("Problem description must not exceed 5000 characters.");
    }
}