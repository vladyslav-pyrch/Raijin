using Raijin.ProblemSolvingService.Application.Features.CommonSat.Commands.SolveSatExpression;
using Raijin.ProblemSolvingService.Domain.SatProblems;
using Xunit.Abstractions;

namespace Raijin.ProblemSolvingService.Application.Tests.Features.CommonSat.Commands.SolveSatExpression;

public class SolveSatExpressionCommandHandlerTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void Test()
    {
        var parser = new SatExpressionParser();
        var satProblem = new SatProblem();

        parser.ParseInto("(v d ea)(e ~a ~ea)", satProblem);

        testOutputHelper.WriteLine(satProblem.ToDimacsString());
    }
}