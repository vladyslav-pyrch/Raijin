using Raijin.ProblemSolvingService.Api.Endpoints.V1.SolveSatExpression;
using Raijin.ProblemSolvingService.Api.Endpoints.V1.SolveSatProblem;

namespace Raijin.ProblemSolvingService.Api.Endpoints.V1;

public static class EndpointsV1Config
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/sat/solve", SolveSatProblemEndpoint.Execute);
        app.MapPost("/v1/sat/solve-expression", SolveSatExpressionEndpoint.Execute);

        return app;
    }
}