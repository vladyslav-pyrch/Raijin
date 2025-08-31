using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using Raijin.ProblemSolvingService.Api.Endpoints.V1.CommonSat.Shared.Requests;
using Raijin.ProblemSolvingService.Api.Endpoints.V1.CommonSat.Shared.Responses;
using Raijin.ProblemSolvingService.Api.Endpoints.V1.CommonSat.SolveSatProblem;
using Raijin.ProblemSolvingService.Application.Cqrs;
using Raijin.ProblemSolvingService.Application.Features.CommonSat.Commands.SolveSatProblem;
using Raijin.ProblemSolvingService.Application.Features.CommonSat.Dtos;

namespace Raijin.ProblemSolvingService.Api.Tests.Endpoints.V1.CommonSat.SolveSatProblem;

[Trait("Category", "Unit")]
public class SolveSatProblemEndpointTests
{
    [Fact]
    public async Task GivenValidRequest_WhenExecuting_ThenPassesToCommandDispatcher()
    {
        var request = new SolveSatProblemRequest(Clauses: [
            new ClauseRequest(Literals: [
                new LiteralRequest(VariableNumber: 1, IsNegated: false)
            ])
        ]);

        var commandDispatcher = Substitute.For<ISender>();
        commandDispatcher.Send(
            request: Arg.Is<SolveSatProblemCommand>(c => c.Clauses.Count == 1),
            cancellationToken: Arg.Any<CancellationToken>()
        ).Returns(new SolveSatProblemCommandResult(
            SolvingStatusDto.Satisfiable,
            VariableAssignments: [new SatVariableAssignmentDto(VariableNumber: 1, Assignment: true)]
        ));

        Results<Ok<SolveSatProblemResponse>, ValidationProblem> results = await SolveSatProblemEndpoint.Execute(
            request, commandDispatcher, CancellationToken.None
        );

        results.Result.Should().BeOfType<Ok<SolveSatProblemResponse>>();
        var result = (Ok<SolveSatProblemResponse>)results.Result;
        result.Value.Should().BeEquivalentTo(new SolveSatProblemResponse(
            SolvingStatusResponse.Satisfiable,
            VariableAssignments: [new VariableAssignmentResponse(VariableNumber: 1, Assignment: true)]
        ));
        await commandDispatcher.Received(1).Send(
            request: Arg.Is<SolveSatProblemCommand>(c => c.Clauses.Count == 1),
            cancellationToken: Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task GivenCancellationToken_WhenExecuting_ThenPassesToCommandDispatcher()
    {
        var request = new SolveSatProblemRequest(Clauses: [
            new ClauseRequest(Literals: [
                new LiteralRequest(VariableNumber: 1, IsNegated: false)
            ])
        ]);

        var commandDispatcher = Substitute.For<ISender>();
        commandDispatcher.Send(
            request: Arg.Any<SolveSatProblemCommand>(),
            cancellationToken: Arg.Any<CancellationToken>()
        ).Returns(new SolveSatProblemCommandResult(
            SolvingStatusDto.Satisfiable,
            VariableAssignments: [new SatVariableAssignmentDto(VariableNumber: 1, Assignment: true)]
        ));

        var cancellationToken = new CancellationToken(canceled: false);

        Results<Ok<SolveSatProblemResponse>, ValidationProblem> results =
            await SolveSatProblemEndpoint.Execute(
                request, commandDispatcher, cancellationToken
            );

        results.Result.Should().BeOfType<Ok<SolveSatProblemResponse>>();
        await commandDispatcher.Received(1).Send(
            request: Arg.Any<SolveSatProblemCommand>(),
            cancellationToken: cancellationToken
        );
    }

    [Fact]
    public async Task GivenEmptyClauses_WhenExecuting_ThenReturnsBadRequest()
    {
        var request = new SolveSatProblemRequest(Clauses: []);

        var commandDispatcher = Substitute.For<ISender>();

        Results<Ok<SolveSatProblemResponse>, ValidationProblem> results =
            await SolveSatProblemEndpoint.Execute(
                request, commandDispatcher, CancellationToken.None
            );

        results.Result.Should().BeOfType<ValidationProblem>();
        await commandDispatcher.DidNotReceive().Send(
            request: Arg.Any<SolveSatProblemCommand>(),
            cancellationToken: Arg.Any<CancellationToken>()
        );
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GivenInvalidVariableNumber_WhenExecuting_ThenReturnsBadRequest(int variableNumber)
    {
        var request = new SolveSatProblemRequest(Clauses: [
            new ClauseRequest(Literals: [
                new LiteralRequest(variableNumber, IsNegated: false)
            ])
        ]);

        var commandDispatcher = Substitute.For<ISender>();

        Results<Ok<SolveSatProblemResponse>, ValidationProblem> results =
            await SolveSatProblemEndpoint.Execute(
                request, commandDispatcher, CancellationToken.None
            );

        results.Result.Should().BeOfType<ValidationProblem>();
        await commandDispatcher.DidNotReceive().Send(
            request: Arg.Any<SolveSatProblemCommand>(),
            cancellationToken: Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task GivenEmptyClause_WhenExecuting_ThenReturnsBadRequest()
    {
        var request = new SolveSatProblemRequest(Clauses: [
            new ClauseRequest(Literals: [])
        ]);

        var commandDispatcher = Substitute.For<ISender>();

        Results<Ok<SolveSatProblemResponse>, ValidationProblem> results =
            await SolveSatProblemEndpoint.Execute(
                request, commandDispatcher, CancellationToken.None
            );

        results.Result.Should().BeOfType<ValidationProblem>();
        await commandDispatcher.DidNotReceive().Send(
            request: Arg.Any<SolveSatProblemCommand>(),
            cancellationToken: Arg.Any<CancellationToken>()
        );
    }
}