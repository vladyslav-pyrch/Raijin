using FluentResults;
using FluentValidation;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.BooleanSatisfiabilityProblems;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Application.Features.BooleanSatisfiabilityProblems.
    CreateBooleanSatisfiabilityProblem;

public sealed record CreateBooleanSatisfiabilityProblemCommand(
    Guid ProblemId,
    IEnumerable<IEnumerable<int>> Clauses
) : IRequest<CreateBooleanSatisfiabilityProblemResult>;

public sealed record CreateBooleanSatisfiabilityProblemResult(Guid ProblemId);

public sealed class CreateBooleanSatisfiabilityProblemHandler(
    IProblemRepository problemRepository,
    IUnitOfWork unitOfWork,
    IMessageBus messageBus
) : IRequestHandler<CreateBooleanSatisfiabilityProblemCommand, CreateBooleanSatisfiabilityProblemResult>
{
    public async Task<Result<CreateBooleanSatisfiabilityProblemResult>> Handle(
        CreateBooleanSatisfiabilityProblemCommand request,
        CancellationToken cancellationToken
    )
    {
        var clauses = request.Clauses
            .Select(clause => new Clause(
                clause.Select(literal => new Literal(
                    new SatVariable(Math.Abs(literal)),
                    literal < 0
                )).ToList()
            )).ToList();
        var instance = new BooleanSatisfiabilityInstance(clauses);

        Problem problem = await problemRepository.GetById(request.ProblemId, cancellationToken);

        problem.SetInstance(instance);

        await problemRepository.Add(problem, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        return new CreateBooleanSatisfiabilityProblemResult(problem.Id);
    }
}

public sealed class CreateBooleanSatisfiabilityProblemValidator
    : AbstractValidator<CreateBooleanSatisfiabilityProblemCommand>
{
    public CreateBooleanSatisfiabilityProblemValidator()
    {
        RuleFor(x => x.Clauses)
            .NotEmpty()
            .Must(clauses => clauses.All(clause => clause.Any()))
            .WithMessage("Each clause must contain at least one literal.");

        RuleForEach(x => x.Clauses)
            .Must(clause => clause.All(literal => literal != 0))
            .WithMessage("Literals cannot be zero.");
    }
}