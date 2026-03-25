using Raijin.SatSolver.Application.Messaging;

namespace Raijin.SatSolver.Infrastructure.Messaging;

public sealed class CorrelationContextAccessor : ICorrelationContextAccessor
{
    private static readonly AsyncLocal<CorrelationContext?> Current = new();

    public CorrelationContext CorrelationContext
    {
        get => Current.Value ?? throw new InvalidOperationException(
            "CorrelationContext has not been initialized for the current execution flow.");
        set => Current.Value = value ?? throw new ArgumentNullException(nameof(value));
    }
}