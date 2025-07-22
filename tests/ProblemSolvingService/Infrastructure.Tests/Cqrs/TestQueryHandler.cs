using Raijin.ProblemSolvingService.Application.Cqrs;

namespace Raijin.ProblemSolvingService.Infrastructure.Tests.Cqrs;

public class TestQueryHandler : IQueryHandler<TestQuery, TestResult>
{
    public int NumberOfTimesCalled { get; private set; }

    public Task<TestResult> Handle(TestQuery query, CancellationToken cancellationToken = default)
    {
        NumberOfTimesCalled++;

        return Task.FromResult(new TestResult());
    }
}