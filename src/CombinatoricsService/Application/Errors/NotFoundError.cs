using FluentResults;

namespace Raijin.CombinatoricsService.Application.Errors;

public class NotFoundError(string entityName, Guid id) : Error($"{entityName} '{id}' was not found.")
{
    public string EntityName => entityName;

    public Guid Id => id;
}
