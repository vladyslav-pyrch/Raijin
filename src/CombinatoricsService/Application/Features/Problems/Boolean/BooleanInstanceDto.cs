using FluentValidation;

namespace Raijin.CombinatoricsService.Application.Features.Problems.Boolean;

public sealed record BooleanProblemInstanceDto(string Formula);

public sealed class BooleanProblemInstanceDtoValidator : AbstractValidator<BooleanProblemInstanceDto>
{
    public BooleanProblemInstanceDtoValidator()
    {
        RuleFor(dto => dto.Formula).NotEmpty();
    }
}