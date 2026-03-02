using FluentResults;

namespace Raijin.SatSolver.Application.Errors;

public class ValidationError(string propertyName, string problem) : Error($"{propertyName}: {problem}")
{
    public string PropertyName => propertyName;
    
    public string Problem => problem;
}