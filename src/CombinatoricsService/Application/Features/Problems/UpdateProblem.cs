using FluentResults;
using FluentValidation;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Factories;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Application.Features.Problems;

public sealed class UpdateProblemHandler(
    IProblemRepository problemRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateProblemCommand>
{
    public async Task<Result> Handle(UpdateProblemCommand request, CancellationToken cancellationToken)
    {
        Problem? problem = await problemRepository.GetById(request.ProblemId, cancellationToken);

        if (problem is null)
            return new NotFoundError(nameof(Problem), request.ProblemId);

        if (request.Name is not null)
            problem.UpdateName(request.Name);

        if (request.Description is not null)
            problem.UpdateDescription(request.Description);

        await problemRepository.Update(problem, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        return Result.Ok();
    }
}

public sealed record UpdateProblemCommand(
    Guid ProblemId,
    string? Name,
    string? Description
) : IRequest;

public sealed class UpdateProblemValidator : AbstractValidator<UpdateProblemCommand>
{
    public UpdateProblemValidator()
    {
        RuleFor(command => command.ProblemId)
            .NotEmpty();
        RuleFor(command => command.Name)
            .MaximumLength(100);
        RuleFor(command => command.Description)
            .MaximumLength(5000);
    }
}