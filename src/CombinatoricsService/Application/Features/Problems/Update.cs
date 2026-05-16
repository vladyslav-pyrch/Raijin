using FluentResults;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Application.Features.Problems;

public sealed class UpdateProblemHandler(
    IProblemRepository problemRepository,
    IUnitOfWork unitOfWork,
    ILogger<UpdateProblemHandler> logger
) : IRequestHandler<UpdateProblemCommand>
{
    public async Task<Result> Handle(UpdateProblemCommand request, CancellationToken cancellationToken)
    {
        Problem? problem = await problemRepository.GetById(request.ProblemId, cancellationToken);

        if (problem is null)
            return new NotFoundError($"Problem '{request.ProblemId}' not found.");

        if (request.Name is not null)
            problem.UpdateName(request.Name);

        if (request.Description is not null)
            problem.UpdateDescription(request.Description);

        await problemRepository.Update(problem, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        logger.LogInformation(
            "Problem updated. ProblemId={ProblemId} NameChanged={NameChanged} DescriptionChanged={DescriptionChanged}",
            problem.Id,
            request.Name is not null,
            request.Description is not null);

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
            .NotEmpty().WithMessage("Problem identifier is required.");
        RuleFor(command => command.Name)
            .MaximumLength(100).WithMessage("Problem name must not exceed 100 characters.");
        RuleFor(command => command.Description)
            .MaximumLength(5000).WithMessage("Problem description must not exceed 5000 characters.");
    }
}
