using Microsoft.Extensions.DependencyInjection;
using Raijin.ProblemSolvingService.Application.Cqrs;
using Raijin.ProblemSolvingService.Infrastructure.Cqrs;

namespace Raijin.ProblemSolvingService.Infrastructure.Tests.Cqrs;

[Trait("Category", "Integration")]
public class DotNetDiSenderTests : SenderTests
{
    protected override ISender GetSender()
    {
        var services = new ServiceCollection();
        Type requestWithoutResultType = RequestWithoutResult.GetType();
        Type requestWithResultType = RequestWithResult.GetType();
        Type requestWithoutResultHandlerType = typeof(IRequestHandler<>).MakeGenericType(requestWithoutResultType);
        Type requestWithResultHandlerType = typeof(IRequestHandler<,>).MakeGenericType(requestWithResultType, typeof(int));

        services.AddSingleton(requestWithoutResultHandlerType,RequestWithoutResultHandler);
        services.AddSingleton(requestWithResultHandlerType, RequestWithResultHandler);

        ServiceProvider provider = services.BuildServiceProvider();

        return new DotNetDiSender(provider);
    }
}