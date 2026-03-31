using FluentResults;

namespace Raijin.CombinatoricsService.Application.Errors;

public class IllegalOperationError(string message) : Error(message)
{
}