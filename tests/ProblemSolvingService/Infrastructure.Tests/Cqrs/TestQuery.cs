using Njinx.ProblemSolvingService.Application.Cqrs;

namespace Njinx.ProblemSolvingService.Infrastructure.Tests.Cqrs;

public sealed record TestQuery : IQuery<TestResult>;