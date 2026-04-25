using FluentResults;
using FluentValidation;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;
using Raijin.CombinatoricsService.Domain.Problems.BooleanSatisfiability;

namespace Raijin.CombinatoricsService.Application.Features.Problems.BooleanSatisfiability;

public sealed record SetBooleanSatisfiabilityProblemInstanceCommand(
    Guid ProblemId,
    BooleanSatisfiabilityInstanceDto Instance
) : IRequest;

public sealed class SetBooleanSatisfiabilityProblemInstanceHandler(
    IProblemRepository problemRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<SetBooleanSatisfiabilityProblemInstanceCommand>
{
    public async Task<Result> Handle(
        SetBooleanSatisfiabilityProblemInstanceCommand request,
        CancellationToken cancellationToken)
    {
        Problem? problem = await problemRepository.GetById(request.ProblemId, cancellationToken);

        if (problem is null)
            return new NotFoundError(nameof(Problem), request.ProblemId);

        if (problem.SolvingStatus == SolvingStatus.Running)
            return new ConflictError("Cannot change instance while solving is in progress.");

        IEnumerable<List<Literal>> literals = request.Instance.Clauses.Select(clause => clause
            .Select(literalStr =>
            {
                bool negated = literalStr.StartsWith('~');
                string name = negated ? literalStr[1..] : literalStr;
                return new Literal(new SatVariable(name), negated);
            }).ToList());

        var clauses = literals.Select(clauseLiterals => new Clause(clauseLiterals)).ToList();
        problem.SetInstance(new BooleanSatisfiabilityInstance(clauses));

        await problemRepository.Update(problem, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        return Result.Ok();
    }
}

public sealed record BooleanSatisfiabilityInstanceDto(IEnumerable<IEnumerable<string>> Clauses);

public sealed class BooleanSatisfiabilityInstanceDtoValidator : AbstractValidator<BooleanSatisfiabilityInstanceDto>
{
    public BooleanSatisfiabilityInstanceDtoValidator()
    {
        RuleFor(instance => instance.Clauses)
            .NotNull()
            .WithMessage("Clauses must not be null.");

        RuleForEach(instance => instance.Clauses)
            .NotEmpty()
            .WithMessage("Each clause must contain at least one literal.")
            .Must(clause => clause.All(lit => lit is not null && SatVariable.IsValidLiteralString(lit)))
            .WithMessage(
                "Every literal must be a valid named variable literal. " +
                "Format: optional leading '~' for negation, followed by a variable name. " +
                "Names must start with alphanumeric or a dash/underscore run followed by alphanumeric. " +
                "Separator types ('-', '_', ':', '::', ':::') cannot be mixed within a single run. " +
                "Names must end with an alphanumeric character.");
    }
}

public sealed class SetBooleanSatisfiabilityProblemInstanceValidator
    : AbstractValidator<SetBooleanSatisfiabilityProblemInstanceCommand>
{
    public SetBooleanSatisfiabilityProblemInstanceValidator()
    {
        RuleFor(c => c.ProblemId).NotEmpty();
        RuleFor(c => c.Instance)
            .NotNull()
            .SetValidator(new BooleanSatisfiabilityInstanceDtoValidator());
    }
}
