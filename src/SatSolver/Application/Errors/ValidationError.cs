using FluentResults;

namespace Raijin.SatSolver.Application.Errors;

public sealed class ValidationError(string propertyName, string problem) : Error($"{propertyName}: {problem}")
{
    public string PropertyName => propertyName;
    
    public string Problem => problem;
}