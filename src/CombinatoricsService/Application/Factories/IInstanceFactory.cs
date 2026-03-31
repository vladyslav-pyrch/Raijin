using FluentResults;
using Raijin.CombinatoricsService.Application.Features.Problems;
using Raijin.CombinatoricsService.Domain.Problems;

namespace Raijin.CombinatoricsService.Application.Factories;

public interface IInstanceFactory
{
    public string ProblemType { get; }

    public Result<Instance> CreateInstance(InstanceDto instanceDto);
}