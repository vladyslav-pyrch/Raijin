using Njinx.ProblemSolvingService.Application.Cqrs;

namespace Njinx.ProblemSolvingService.Infrastructure.Tests.Cqrs;

public sealed class TestCommandHandler : ICommandHandler<TestCommand>
{
    public int NumberOfTimesCalled { get; private set; }

    public Task Handle(TestCommand command, CancellationToken cancellationToken = default)
    {
        NumberOfTimesCalled++;

        return Task.CompletedTask;
    }
}

public sealed  class TestCommandWithResultHandler : ICommandHandler<TestCommandWithResult, TestResult>
{
    public int NumberOfTimesCalled { get; private set; }

    public Task<TestResult> Handle(TestCommandWithResult command, CancellationToken cancellationToken = default)
    {
        NumberOfTimesCalled++;

        return Task.FromResult(new TestResult());
    }
}