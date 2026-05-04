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

public sealed class CreateBooleanProblemHandler(
    IBoolExprParser parser,
    IProblemRepository problemRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateBooleanProblemCommand, CreateBooleanProblemResult>
{
    public async Task<Result<CreateBooleanProblemResult>> Handle(
        CreateBooleanProblemCommand request,
        CancellationToken cancellationToken)
    {
        Result<BoolExpr> parseResult = parser.Parse(request.Instance.Formula);

        if (parseResult.IsFailed)
            return parseResult
                .MapErrors(e => new ValidationError(
                    $"{nameof(request.Instance)}.{nameof(BooleanProblemInstanceDto.Formula)}",
                    e.Message))
                .ToResult();

        var problem = Problem.Create(
            Guid.CreateVersion7(),
            request.Details.Name,
            request.Details.Description,
            new BooleanProblemInstance(parseResult.Value)
        );

        await problemRepository.Add(problem, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        return new CreateBooleanProblemResult(problem.Id);
    }
}

public sealed record CreateBooleanProblemCommand(
    ProblemDetailsDto Details,
    BooleanProblemInstanceDto Instance
) : IRequest<CreateBooleanProblemResult>;

public sealed record CreateBooleanProblemResult(
    Guid ProblemId
);

public sealed class CreateBooleanProblemValidator : AbstractValidator<CreateBooleanProblemCommand>
{
    public CreateBooleanProblemValidator(
        IValidator<ProblemDetailsDto> problemDetailsValidator,
        IValidator<BooleanProblemInstanceDto> booleanProblemInstanceValidator)
    {
        RuleFor(dto => dto.Details)
            .NotNull()
            .SetValidator(problemDetailsValidator);
        RuleFor(c => c.Instance)
            .NotNull()
            .SetValidator(booleanProblemInstanceValidator);
    }
}