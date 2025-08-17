using FluentAssertions;
using NSubstitute;
using Raijin.ProblemSolvingService.Application.Cqrs;

namespace Raijin.ProblemSolvingService.Infrastructure.Tests.Cqrs;

public abstract class SenderTests
{
    public SenderTests()
    {
        RequestWithoutResult = Substitute.For<IRequest>();
        RequestWithResult = Substitute.For<IRequest<int>>();
        RequestWithoutResultHandler = Substitute.For<IRequestHandler<IRequest>>();
        RequestWithResultHandler = Substitute.For<IRequestHandler<IRequest<int>, int>>();

        RequestWithoutResultHandler.Handle(
            request: Arg.Any<IRequest>(),
            cancellationToken: Arg.Any<CancellationToken>()
            );
        RequestWithResultHandler.Handle(
            request: Arg.Any<IRequest<int>>(),
            cancellationToken: Arg.Any<CancellationToken>()
        ).Returns(1);
    }

    protected IRequest RequestWithoutResult { get; }

    protected IRequest<int> RequestWithResult { get; }

    protected IRequestHandler<IRequest> RequestWithoutResultHandler { get; }

    protected IRequestHandler<IRequest<int>, int> RequestWithResultHandler { get; }

    protected abstract ISender GetSender();

    [Fact]
    public async Task GivenRequestWithResult_WhenSending_SendsToHandler()
    {
        ISender sender = GetSender();

        int result = await sender.Send(RequestWithResult, CancellationToken.None);

        result.Should().Be(1);
        await RequestWithResultHandler.Received(1).Handle(RequestWithResult, CancellationToken.None);
    }

    [Fact]
    public async Task GivenRequestWithoutResult_WhenSending_SendsToHandler()
    {
        ISender sender = GetSender();

        await sender.Send(RequestWithoutResult, CancellationToken.None);

        await RequestWithoutResultHandler.Received(1).Handle(RequestWithoutResult, CancellationToken.None);
    }
}