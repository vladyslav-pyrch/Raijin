using FluentResults;
using FluentValidation;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Application.Features.Problems;

public sealed class DeleteProblemHandler(
    IProblemRepository problemRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<DeleteProblemCommand, DeleteProblemResult>
{
    public async Task<Result<DeleteProblemResult>> Handle(
        DeleteProblemCommand request,
        CancellationToken cancellationToken)
    {
        Problem? problem = await problemRepository.GetById(request.ProblemId, cancellationToken);

        if (problem is null)
            return new NotFoundError($"Problem '{request.ProblemId}' not found.");

        if (problem.SolvingStatus is SolvingStatus.Running)
            return new ConflictError("Cannot delete problem while solving is in progress.");

        await problemRepository.Delete(problem.Id, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        return new DeleteProblemResult(problem.Id);
    }
}

public sealed record DeleteProblemCommand(
    Guid ProblemId
) : IRequest<DeleteProblemResult>;

public sealed record DeleteProblemResult(
    Guid ProblemId
);

public sealed class DeleteProblemValidator : AbstractValidator<DeleteProblemCommand>
{
    public DeleteProblemValidator()
    {
        RuleFor(command => command.ProblemId)
            .NotEmpty().WithMessage("Problem identifier is required.");
    }
}
