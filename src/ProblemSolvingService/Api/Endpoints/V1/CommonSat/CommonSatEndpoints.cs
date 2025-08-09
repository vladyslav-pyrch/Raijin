using Raijin.ProblemSolvingService.Api.Endpoints.V1.CommonSat.SolveSatProblem;

namespace Raijin.ProblemSolvingService.Api.Endpoints.V1.CommonSat;

public static class CommonSatEndpoints
{
    public static IEndpointRouteBuilder MapCommonSatEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/common-sat/solve", SolveSatProblemEndpoint.Execute);

        return app;
    }
}