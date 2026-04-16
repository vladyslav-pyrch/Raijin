using FluentResults;
using FluentValidation;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Factories;
using Raijin.CombinatoricsService.Application.Parsing;
using Raijin.CombinatoricsService.Application.Validation;
using Raijin.CombinatoricsService.Domain.BooleanExpressions;
using Raijin.CombinatoricsService.Domain.Problems;
using Raijin.CombinatoricsService.Domain.Problems.Boolean;

namespace Raijin.CombinatoricsService.Application.Features.Problems.Boolean;

public sealed class BooleanProblemInstanceFactory(
    IBoolExprParser parser,
    IValidator<BooleanProblemInstanceDto>? validator = null
) : IInstanceFactory
{
    public string ProblemType => ProblemTypes.BooleanProblem;

    public Result<Instance> CreateInstance(InstanceDto dto)
    {
        if (dto is not BooleanProblemInstanceDto booleanDto)
            throw new ArgumentException(
                $"Invalid instance DTO type. Expected {typeof(BooleanProblemInstanceDto)}, got {dto.GetType()}");

        var validationResult = Result.FailIfNotEmpty(
            validator?.Validate(booleanDto).ToValidationErrors() ?? []
        );

        if (validationResult.IsFailed)
            return validationResult;

        Result<BoolExpr> parseResult = parser.Parse(booleanDto.Formula);

        if (parseResult.IsFailed)
            return parseResult
                .MapErrors(e => new ValidationError(nameof(BooleanProblemInstanceDto.Formula), e.Message))
                .ToResult<Instance>();

        return Result.Ok<Instance>(new BooleanProblemInstance(parseResult.Value));
    }
}
