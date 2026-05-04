using System.Text;

namespace Raijin.CombinatoricsService.Infrastructure.Solvers.Cadical;

internal enum CadicalPreset { Default, Plain, Sat, Unsat }

/// <summary>
///     Fluent builder for CaDiCaL CLI arguments.
///     Only options that are explicitly set are emitted; the executable uses its own defaults for the rest.
/// </summary>
internal sealed class CadicalArgumentsBuilder
{
    private long? _conflictLimit;
    private long? _decisionLimit;
    private bool _noColors;
    private bool _noWitness;
    private CadicalPreset? _preset;
    private bool _quiet;
    private int? _verbose;

    public CadicalArgumentsBuilder WithNoWitness()
    {
        _noWitness = true;
        return this;
    }

    public CadicalArgumentsBuilder WithQuiet()
    {
        _quiet = true;
        return this;
    }

    public CadicalArgumentsBuilder WithVerbosity(int level)
    {
        _verbose = level;
        return this;
    }

    public CadicalArgumentsBuilder WithConflictLimit(long limit)
    {
        _conflictLimit = limit;
        return this;
    }

    public CadicalArgumentsBuilder WithDecisionLimit(long limit)
    {
        _decisionLimit = limit;
        return this;
    }

    public CadicalArgumentsBuilder WithNoColors()
    {
        _noColors = true;
        return this;
    }

    public CadicalArgumentsBuilder WithPreset(CadicalPreset preset)
    {
        _preset = preset;
        return this;
    }

    public string Build()
    {
        var sb = new StringBuilder();

        if (_noWitness)
            sb.Append("-n ");
        if (_quiet)
            sb.Append("-q ");
        if (_verbose.HasValue)
            sb.Append($"--verbose={_verbose} ");
        if (_conflictLimit.HasValue)
            sb.Append($"-c {_conflictLimit} ");
        if (_decisionLimit.HasValue)
            sb.Append($"-d {_decisionLimit} ");
        if (_noColors)
            sb.Append("--no-colors ");
        if (_preset.HasValue)
            sb.Append($"--{_preset.Value.ToString().ToLowerInvariant()} ");

        return sb.ToString().TrimEnd();
    }
}