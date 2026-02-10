using Microsoft.Extensions.DependencyInjection;
using Raijin.ProblemSolvingService.Application.Cqrs;
using Raijin.ProblemSolvingService.Infrastructure.Cqrs;

namespace Raijin.ProblemSolvingService.Infrastructure;

public static class InfrastructureModule
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddCqrs();
        services.AddSatSolver();

        return services;
    }

    private static IServiceCollection AddCqrs(this IServiceCollection services) =>
        services.AddScoped<ISender, DotNetDiSender>();

    private static IServiceCollection AddSatSolver(this IServiceCollection services)
    {
        // services.AddScoped<ISatSolver, CryptominisatSatSolver>();

        return services;
    }
}