using Raijin.ProblemSolvingService.Application.Cqrs;

namespace Raijin.ProblemSolvingService.Infrastructure.Tests.Cqrs;

public sealed record TestQuery : IQuery<TestResult>;