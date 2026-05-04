using FluentResults;

namespace Raijin.CombinatoricsService.Infrastructure.Solvers.Cadical;

internal interface ICadicalCli
{
    Task<Result<string>> ExecuteAsync(
        string cnfFilePath,
        CadicalArgumentsBuilder arguments,
        CancellationToken cancellationToken);
}