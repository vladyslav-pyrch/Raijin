using FluentResults;
using Microsoft.Extensions.Logging;
using Raijin.Application.Contracts;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Domain.CombinatoricProblems;

namespace Raijin.CombinatoricsService.Application.Features.SubmitCombinatoricProblem;

public sealed class SubmitCombinatoricProblemHandler(
    ICombinatoricProblemRepository combinatoricProblemRepository,
    IUnitOfWork unitOfWork,
    IMessageBus messageBus,
    ILogger<SubmitCombinatoricProblemHandler> logger
) : IRequestHandler<SubmitCombinatoricProblemCommand, SubmitCombinatoricProblemResult>
{
    public async Task<Result<SubmitCombinatoricProblemResult>> Handle(
        SubmitCombinatoricProblemCommand request,
        CancellationToken cancellationToken
    )
    {
        var combinatoricProblemId = Guid.CreateVersion7();
        IEnumerable<(string, string[])> decisionVariables =
            request.DecisionVariables.Select(variable => (variable.Name, variable.States));

        CombinatoricProblem combinatoricProblem = new(combinatoricProblemId);
        combinatoricProblem.AddDecisionVariables(decisionVariables);
        combinatoricProblem.AddConstrains(request.Constraints);

        await combinatoricProblemRepository.Add(combinatoricProblem, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        await messageBus.Publish<ICombinatoricProblemSubmitted>(new
        {
            CombinatoricProblemId = combinatoricProblemId,
            DecisionVariables =
                combinatoricProblem.DecisionVariables.Select(variable => new { variable.Name, variable.States })
                    .ToArray(),
            Constraints = combinatoricProblem.Constraints.Select(constraint => constraint.Formula).ToArray()
        }, cancellationToken);

        return new SubmitCombinatoricProblemResult(combinatoricProblemId);
    }
}