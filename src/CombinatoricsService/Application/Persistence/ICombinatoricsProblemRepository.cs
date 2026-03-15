using Raijin.CombinatoricsService.Domain.CombinatoricProblems;

namespace Raijin.CombinatoricsService.Application.Persistence;

public interface ICombinatoricsProblemRepository
{
    public Task Add(CombinatoricProblem problem, CancellationToken cancellationToken);
    
    public Task Save(CancellationToken cancellationToken);
}