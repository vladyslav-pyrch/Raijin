using Njinx.ProblemSolvingService.Application.Cqrs;

namespace Njinx.ProblemSolvingService.Infrastructure.Tests.Cqrs;

public sealed class TestCommand : ICommand;

public sealed class TestCommandWithResult : ICommand<TestResult>;