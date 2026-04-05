using FluentResults;
using FluentValidation;
using Raijin.SatSolver.Application.Messaging;
using Raijin.SatSolver.Application.Persistence;
using Raijin.SatSolver.Domain.SatProblems;

namespace Raijin.SatSolver.Application.Features.SatProblems;

public sealed class CreateSatProblemHandler(
    ISatProblemJobRepository satProblemJobRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateSatProblemCommand>
{
    public async Task<Result> Handle(CreateSatProblemCommand request, CancellationToken cancellationToken)
    {
        var satProblem = SatProblem.Create(
            request.SatProblemId,
            request.Clauses
        );

        await satProblemJobRepository.Add(satProblem, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        return Result.Ok();
    }
}

public sealed record CreateSatProblemCommand(Guid SatProblemId, IEnumerable<IEnumerable<int>> Clauses) : IRequest;

public sealed class CreateSatProblemValidator : AbstractValidator<CreateSatProblemCommand>
{
    public CreateSatProblemValidator()
    {
        RuleFor(command => command.Clauses)
            .NotEmpty();

        RuleForEach(command => command.Clauses)
            .NotEmpty()
            .Must(literals => literals.All(literal => literal != 0))
            .WithMessage("A literal may not be zero");
    }
}