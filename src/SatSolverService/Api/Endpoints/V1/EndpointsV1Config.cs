using Raijin.SatSolverService.Api.Endpoints.V1.SatProblems.SolveSat;

namespace Raijin.SatSolverService.Api.Endpoints.V1;

public static class EndpointsV1Config
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("api/v1/sat_problems", SolveSatEndpoint.Execute);
        app.MapGet("api/v1/sat_problems/{id}", (int id) => id);

        return app;
    }
}