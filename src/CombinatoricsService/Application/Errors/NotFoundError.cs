using FluentResults;

namespace Raijin.CombinatoricsService.Application.Errors;

public sealed class NotFoundError : Error
{
    public NotFoundError(string message) : base(message)
    {
    }
}