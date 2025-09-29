using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using Raijin.ProblemSolvingService.Api.Endpoints.V1.SolveSatExpression;
using Raijin.ProblemSolvingService.Application.Cqrs;
using Raijin.ProblemSolvingService.Application.Features.Dtos;
using Raijin.ProblemSolvingService.Application.Features.SolveSatExpression;

namespace Raijin.ProblemSolvingService.Api.Tests.Endpoints.V1.SolveSatExpression;

[Trait("Category", "Unit")]
public class SolveSatExpressionEndpointTests
{
    [Fact]
    public async Task GivenValidRequest_WhenExpressionIsSatisfiable_ThenReturnsOkWithSolution()
    {
        var request = new SolveSatExpressionRequest(SatExpression: "(a b) (a ~b) (b)");
        var commandResult = new SolveSatExpressionResult(SolvingStatusDto.Satisfiable, [
            new NamedSatVariableAssignmentDto("a", true),
            new NamedSatVariableAssignmentDto("b", true)
        ]);

        var sender = Substitute.For<ISender>();
        sender.Send(Arg.Any<SolveSatExpressionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(commandResult));

        Results<Ok<SolveSatExpressionResponse>, ValidationProblem> response =
            await SolveSatExpressionEndpoint.Execute(request, sender);

        response.Result.Should().BeOfType<Ok<SolveSatExpressionResponse>>();
        var okResult = response.Result as Ok<SolveSatExpressionResponse>;
        okResult!.Value.Should().BeEquivalentTo(SolveSatExpressionResponse.From(commandResult));
    }

    [Fact]
    public async Task GivenValidRequest_WhenExpressionIsUnsatisfiable_ThenReturnsOkWithSolution()
    {
        var request = new SolveSatExpressionRequest(SatExpression: "(a b) (a ~b) (b) (~a)");
        var commandResult = new SolveSatExpressionResult(SolvingStatusDto.Unsatisfiable, []);
        var sender = Substitute.For<ISender>();
        sender.Send(Arg.Any<SolveSatExpressionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(commandResult));

        Results<Ok<SolveSatExpressionResponse>, ValidationProblem> response =
            await SolveSatExpressionEndpoint.Execute(request, sender);

        response.Result.Should().BeOfType<Ok<SolveSatExpressionResponse>>();
        var okResult = response.Result as Ok<SolveSatExpressionResponse>;
        okResult!.Value.Should().BeEquivalentTo(SolveSatExpressionResponse.From(commandResult));
    }

    [Fact]
    public async Task GivenValidRequest_WhenExpressionIsIndeterminate_ThenReturnsOkWithSolution()
    {
        var request = new SolveSatExpressionRequest(SatExpression: "(a b) (a ~b) (b) (~a)");
        var commandResult = new SolveSatExpressionResult(SolvingStatusDto.Indeterminate, []);
        var sender = Substitute.For<ISender>();
        sender.Send(Arg.Any<SolveSatExpressionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(commandResult));

        Results<Ok<SolveSatExpressionResponse>, ValidationProblem> response =
            await SolveSatExpressionEndpoint.Execute(request, sender);

        response.Result.Should().BeOfType<Ok<SolveSatExpressionResponse>>();
        var okResult = response.Result as Ok<SolveSatExpressionResponse>;
        okResult!.Value.Should().BeEquivalentTo(SolveSatExpressionResponse.From(commandResult));
    }

    [Fact]
    public async Task GivenInvalidRequest_WhenExpressionCannotBeParsed_ThenReturnsValidationProblem()
    {
        var request = new SolveSatExpressionRequest(SatExpression: "(a b (a ~b) (b)");
        var sender = Substitute.For<ISender>();
        sender.Send(Arg.Any<SolveSatExpressionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Fail<SolveSatExpressionResult>(new SatParseError("Expected ')'", 4)));

        Results<Ok<SolveSatExpressionResponse>, ValidationProblem> response =
            await SolveSatExpressionEndpoint.Execute(request, sender);

        response.Result.Should().BeOfType<ValidationProblem>();
        var validationProblem = response.Result as ValidationProblem;
        validationProblem!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        validationProblem!.ProblemDetails.Errors.Should().ContainKey("SatExpression");
        validationProblem!.ProblemDetails.Errors["SatExpression"].Should().Contain("Error at 4; Expected ')'");
    }
}