using FluentResults;

namespace Raijin.CombinatoricsService.Infrastructure.Solvers.Cryptominisat;

internal interface ICryptominisatCli
{
    Task<Result<string>> ExecuteAsync(string cnfFilePath, CryptominisatArgumentsBuilder arguments, CancellationToken cancellationToken);
}