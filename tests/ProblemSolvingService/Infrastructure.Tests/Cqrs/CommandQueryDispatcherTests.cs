using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Njinx.ProblemSolvingService.Application.Cqrs;
using Njinx.ProblemSolvingService.Infrastructure.Cqrs;

namespace Njinx.ProblemSolvingService.Infrastructure.Tests.Cqrs;

public class CommandQueryDispatcherTests
{
    [Fact]
    public async Task GivenServiceProviderDoesNotHaveHandler_WhenDispatchingCommand_ThenThrowInvalidOperationException()
    {
        ServiceProvider provider = new ServiceCollection().BuildServiceProvider();
        ICommandDispatcher dispatcher = new CommandQueryDispatcher(provider);

        Func<Task> when = async () => await dispatcher.Dispatch(new TestCommand());

        await when.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GivenServiceProviderDoesNotHaveHandler_WhenDispatchingCommandWithResult_ThenThrowInvalidOperationException()
    {
        ServiceProvider provider = new ServiceCollection().BuildServiceProvider();
        ICommandDispatcher dispatcher = new CommandQueryDispatcher(provider);

        Func<Task> when = async () => _ = await dispatcher.Dispatch<TestCommandWithResult, TestResult>(
            new TestCommandWithResult()
        );

        await when.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GivenServiceProviderDoesNotHaveHandler_WhenDispatchingQuery_ThenThrowInvalidOperationException()
    {
        ServiceProvider provider = new ServiceCollection().BuildServiceProvider();
        IQueryDispatcher dispatcher = new CommandQueryDispatcher(provider);

        Func<Task> when = async () => _ = await dispatcher.Dispatch<TestQuery, TestResult>(new TestQuery());

        await when.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GivenServiceProviderHasHandler_WhenDispatchingCommand_ThenHandlerIsCalled()
    {
        IServiceCollection collection = new ServiceCollection();
        collection.AddSingleton<ICommandHandler<TestCommand>, TestCommandHandler>();
        IServiceProvider provider = collection.BuildServiceProvider();
        ICommandDispatcher dispatcher = new CommandQueryDispatcher(provider);

        await dispatcher.Dispatch(new TestCommand());

        var handler = provider.GetRequiredService<ICommandHandler<TestCommand>>() as TestCommandHandler;
        handler!.NumberOfTimesCalled.Should().Be(1);
    }

    [Fact]
    public async Task GivenServiceProviderHasHandler_WhenDispatchingCommandWithResult_ThenHandlerIsCalled()
    {
        IServiceCollection collection = new ServiceCollection();
        collection.AddSingleton<ICommandHandler<TestCommandWithResult, TestResult>, TestCommandWithResultHandler>();
        IServiceProvider provider = collection.BuildServiceProvider();
        ICommandDispatcher dispatcher = new CommandQueryDispatcher(provider);

        await dispatcher.Dispatch<TestCommandWithResult, TestResult>(new TestCommandWithResult());

        var handler = provider.GetRequiredService<ICommandHandler<TestCommandWithResult, TestResult>>() as TestCommandWithResultHandler;
        handler!.NumberOfTimesCalled.Should().Be(1);
    }

    [Fact]
    public async Task GivenServiceProviderHasHandler_WhenDispatchingQuery_ThenHandlerIsCalled()
    {
        IServiceCollection collection = new ServiceCollection();
        collection.AddSingleton<IQueryHandler<TestQuery, TestResult>, TestQueryHandler>();
        IServiceProvider provider = collection.BuildServiceProvider();
        IQueryDispatcher dispatcher = new CommandQueryDispatcher(provider);

        await dispatcher.Dispatch<TestQuery, TestResult>(new TestQuery());

        var handler = provider.GetRequiredService<IQueryHandler<TestQuery, TestResult>>() as TestQueryHandler;
        handler!.NumberOfTimesCalled.Should().Be(1);
    }
}