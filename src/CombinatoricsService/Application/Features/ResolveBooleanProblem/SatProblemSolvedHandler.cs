using Raijin.Application.Contracts;
using Raijin.CombinatoricsService.Application.Messaging;

namespace Raijin.CombinatoricsService.Application.Features.ResolveBooleanProblem;

public class SatProblemSolvedHandler : IMessageHandler<ISatProblemSolved>
{
    public async Task Handle(ISatProblemSolved message, CancellationToken cancellationToken)
    {
    }
}