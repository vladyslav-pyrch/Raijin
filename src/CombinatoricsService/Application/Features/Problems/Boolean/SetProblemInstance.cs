using FluentResults;
using FluentValidation;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Parsing;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.BooleanExpressions;
using Raijin.CombinatoricsService.Domain.Problems;
using Raijin.CombinatoricsService.Domain.Problems.Boolean;

namespace Raijin.CombinatoricsService.Application.Features.Problems.Boolean;

public sealed record SetBooleanProblemInstanceCommand(
    Guid ProblemId,
    BooleanProblemInstanceDto Instance
) : IRequest;

public sealed class SetBooleanProblemInstanceHandler(
    IBoolExprParser parser,
    IProblemRepository problemRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<SetBooleanProblemInstanceCommand>
{
    public async Task<Result> Handle(
        SetBooleanProblemInstanceCommand request,
        CancellationToken cancellationToken)
    {
        Problem? problem = await problemRepository.GetById(request.ProblemId, cancellationToken);

        if (problem is null)
            return new NotFoundError(nameof(Problem), request.ProblemId);

        if (problem.SolvingStatus == SolvingStatus.Running)
            return new ConflictError("Cannot change instance while solving is in progress.");

        Result<BoolExpr> parseResult = parser.Parse(request.Instance.Formula);

        if (parseResult.IsFailed)
            return parseResult
                .MapErrors(e => new ValidationError(
                    $"{nameof(request.Instance)}.{nameof(BooleanProblemInstanceDto.Formula)}",
                    e.Message))
                .ToResult();

        problem.SetInstance(new BooleanProblemInstance(parseResult.Value));

        await problemRepository.Update(problem, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        return Result.Ok();
    }
}

public sealed record BooleanProblemInstanceDto(string Formula);

public sealed class BooleanProblemInstanceDtoValidator : AbstractValidator<BooleanProblemInstanceDto>
{
    public BooleanProblemInstanceDtoValidator()
    {
        RuleFor(dto => dto.Formula).NotEmpty();
    }
}

public sealed class SetBooleanProblemInstanceValidator : AbstractValidator<SetBooleanProblemInstanceCommand>
{
    public SetBooleanProblemInstanceValidator()
    {
        RuleFor(c => c.ProblemId).NotEmpty();
        RuleFor(c => c.Instance)
            .NotNull()
            .SetValidator(new BooleanProblemInstanceDtoValidator());
    }
}
