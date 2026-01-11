using Raijin.ProblemSolvingService.Api.Endpoints.V1.SolveBooleanExpression;
using Raijin.ProblemSolvingService.Api.Endpoints.V1.SolveSatExpression;

namespace Raijin.ProblemSolvingService.Api.Endpoints.V1;

public static class EndpointsV1Config
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/solve-sat-expression", SolveSatExpressionEndpoint.Execute);
        app.MapPost("/v1/solve-boolean-expression", SolveBooleanExpressionEndpoint.Execute);

        return app;
    }
}