using FluentResults;

namespace Raijin.CombinatoricsService.Application.Errors;

public sealed class ValidationError : Error
{
    public ValidationError(string propertyName, string problem) : base(problem)
    {
        PropertyName = propertyName;
        Problem = problem;
        Metadata["field"] = propertyName;
    }

    public string PropertyName { get; }

    public string Problem { get; }
}