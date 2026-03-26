using FluentResults;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.CombinatoricProblems;

namespace Raijin.CombinatoricsService.Application.Features.ResolveCombinatoricProblem;

public sealed class ResolveCombinatoricProblemHandler(
    IEventStore eventStore,
    IMessageBus messageBus,
    ILogger<ResolveCombinatoricProblemHandler> logger
) : IRequestHandler<ResolveCombinatoricProblemCommand>
{
    public async Task<Result> Handle(ResolveCombinatoricProblemCommand request, CancellationToken cancellationToken)
    {
        var combinatoricProblem =
            await eventStore.GetById<CombinatoricProblem>(request.CombinatoricProblemId, cancellationToken);

        if (combinatoricProblem is null)
            return new NotFoundError(nameof(CombinatoricProblem), request.CombinatoricProblemId);

        combinatoricProblem.ResolveVariableAssignments(request.BooleanProblemSolutionSolution.VariableAssignments);

        await eventStore.Save(combinatoricProblem, cancellationToken);

        await messageBus.Publish<ICombinatoricProblemSolved>(new
        {
            CombinatoricProblemId = combinatoricProblem.Id,
            Solution = combinatoricProblem.Solution.ToDictionary(
                assignment => assignment.DecisionVariable.Name,
                assignment => assignment.SelectedState
            )
        }, cancellationToken);

        return Result.Ok();
    }
}