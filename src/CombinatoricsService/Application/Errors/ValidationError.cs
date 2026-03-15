using FluentResults;

namespace Raijin.CombinatoricsService.Application.Errors;

public class ValidationError(string propertyName, string problem) : Error($"{propertyName}: {problem}")
{
    public string PropertyName => propertyName;
    
    public string Problem => problem;
}