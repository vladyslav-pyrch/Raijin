using FluentResults;

namespace Raijin.CombinatoricsService.Application.Solvers;

public sealed class SolverTimeoutError(int timeoutSeconds)
    : Error($"Solver timed out after {timeoutSeconds} second(s).")
{
    public int TimeoutSeconds { get; } = timeoutSeconds;
}
