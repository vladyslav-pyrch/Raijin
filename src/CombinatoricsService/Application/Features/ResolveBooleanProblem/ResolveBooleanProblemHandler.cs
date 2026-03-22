using FluentResults;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.Logic;
using Raijin.CombinatoricsService.Domain.Shared;

namespace Raijin.CombinatoricsService.Application.Features.ResolveBooleanProblem;

public class ResolveBooleanProblemHandler(
    IBooleanProblemRepository booleanProblemRepository,
    IUnitOfWork unitOfWork,
    IMessageBus messageBus,
    ILogger<ResolveBooleanProblemHandler> logger
) : IRequestHandler<ResolveBooleanProblemCommand>
{
    public async Task<Result> Handle(ResolveBooleanProblemCommand request, CancellationToken cancellationToken)
    {
        BooleanProblem? booleanProblem =
            await booleanProblemRepository.GetById(request.BooleanProblemId, cancellationToken);

        if (booleanProblem == null)
            return Result.Fail(new NotFoundError(nameof(BooleanProblem), request.BooleanProblemId));

        Result result = Result.Try(
            () => booleanProblem.ResolveSatSolution(new SatSolution(request.SatSolution)),
            exception => new ValidationError(nameof(request.SatSolution), exception.Message)
        );

        if (result.IsFailed)
            return result;

        await booleanProblemRepository.Update(booleanProblem, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        await messageBus.Publish<IBooleanProblemSolved>(new
        {
            BooleanProblemId = booleanProblem.Id,
            Solution = booleanProblem.Solution!.Assignments.ToDictionary(
                assignment => assignment.Variable.Name,
                assignment => assignment.Value
            )
        }, cancellationToken);

        return result;
    }
}