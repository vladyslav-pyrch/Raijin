using FluentResults;

namespace Raijin.CombinatoricsService.Application.Errors;

public sealed class NotFoundError : Error
{
    public NotFoundError(string entityName, Guid id) : base($"{entityName} '{id}' was not found.")
    {
        EntityName = entityName;
        Id = id;
        Metadata["key"] = id;
    }

    public string EntityName { get; }

    public Guid Id { get; }
}