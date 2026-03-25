using FluentResults;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;
using Raijin.CombinatoricsService.Application.Errors;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.CombinatoricProblems;

namespace Raijin.CombinatoricsService.Application.Features.ResolveCombinatoricProblem;

public sealed class ResolveCombinatoricProblemHandler(
    ICombinatoricProblemRepository combinatoricProblemRepository,
    IUnitOfWork unitOfWork,
    IMessageBus messageBus,
    ILogger<ResolveCombinatoricProblemHandler> logger
) : IRequestHandler<ResolveCombinatoricProblemCommand>
{
    public async Task<Result> Handle(ResolveCombinatoricProblemCommand request, CancellationToken cancellationToken)
    {
        CombinatoricProblem? combinatoricProblem = await combinatoricProblemRepository.GetById(
            request.CombinatoricProblemId,
            cancellationToken
        );

        if (combinatoricProblem is null)
            return new NotFoundError(nameof(CombinatoricProblem), request.CombinatoricProblemId);

        combinatoricProblem.ResolveVariableAssignments(request.BooleanProblemSolutionSolution.VariableAssignments);

        await combinatoricProblemRepository.Update(combinatoricProblem, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        await messageBus.Publish<ICombinatoricProblemSolved>(new
        {
            CombinatoricProblemId = combinatoricProblem.Id,
            Solution = combinatoricProblem.Solution.Assignments.ToDictionary(
                assignment => assignment.DecisionVariable.Name,
                assignment => assignment.SelectedState
            )
        }, cancellationToken);

        return Result.Ok();
    }
}