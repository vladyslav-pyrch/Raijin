using FluentResults;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.BooleanProblems;

namespace Raijin.CombinatoricsService.Application.Features.ResolveBooleanProblem;

public sealed class ResolveBooleanProblemHandler(
    IEventStore eventStore,
    IMessageBus messageBus,
    ILogger<ResolveBooleanProblemHandler> logger
) : IRequestHandler<ResolveBooleanProblemCommand>
{
    public async Task<Result> Handle(ResolveBooleanProblemCommand request, CancellationToken cancellationToken)
    {
        var booleanProblem =
            await eventStore.GetById<BooleanProblem>(request.BooleanProblemId, cancellationToken);

        if (booleanProblem is null)
            return new NotFoundError(nameof(BooleanProblem), request.BooleanProblemId);

        booleanProblem.ResolveSatSolution(request.SatSolution.Literals);

        await eventStore.Save(booleanProblem, cancellationToken);

        await messageBus.Publish<IBooleanProblemSolved>(new
        {
            BooleanProblemId = booleanProblem.Id,
            Solution = booleanProblem.Solution.ToDictionary(
                assignment => assignment.Variable.Name,
                assignment => assignment.Value
            )
        }, cancellationToken);

        return Result.Ok();
    }
}