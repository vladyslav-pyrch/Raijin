using FluentResults;
using FluentValidation;
using Raijin.CombinatoricsService.Application.Factories;
using Raijin.CombinatoricsService.Application.Validation;
using Raijin.CombinatoricsService.Domain.Problems;
using Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;

namespace Raijin.CombinatoricsService.Application.Features.Problems.BooleanSatisfiability;

public class BooleanSatisfiabilityInstanceFactory(
    IValidator<BooleanSatisfiabilityInstanceDto>? validator
) : IInstanceFactory
{
    public string ProblemType => ProblemTypes.BooleanSatisfiabilityProblem;

    public Result<Instance> CreateInstance(InstanceDto instanceDto)
    {
        if (instanceDto is not BooleanSatisfiabilityInstanceDto booleanSatisfiabilityInstanceDto)
            throw new ArgumentException(
                $"Invalid instance DTO type. Expected {typeof(BooleanSatisfiabilityInstanceDto)}, got {instanceDto.GetType()}");

        var result = Result.FailIfNotEmpty(
            validator?.Validate(booleanSatisfiabilityInstanceDto).ToValidationErrors() ?? []
        );

        if (result.IsFailed)
            return result;

        IEnumerable<List<Literal>> literals = booleanSatisfiabilityInstanceDto.Clauses.Select(clause => clause
            .Select(literalStr =>
            {
                bool negated = literalStr.StartsWith('~');
                string name = negated ? literalStr[1..] : literalStr;
                return new Literal(new SatVariable(name), negated);
            }).ToList());

        var clauses = literals.Select(clauseLiterals => new Clause(clauseLiterals)).ToList();
        return new BooleanSatisfiabilityInstance(clauses);
    }
}