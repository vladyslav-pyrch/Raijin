using Raijin.ProblemSolvingService.Application.Cqrs;

namespace Raijin.ProblemSolvingService.Infrastructure.Tests.Cqrs;

public sealed class TestCommand : ICommand;

public sealed class TestCommandWithResult : ICommand<TestResult>;