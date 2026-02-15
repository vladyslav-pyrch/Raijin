using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Raijin.SatSolver.Application.Features.SolveSat;

namespace Raijin.SatSolver.Application;

public static class ApplicationModule
{
    public static readonly Assembly Assembly = typeof(ApplicationModule).Assembly;

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<SolveSatHandler>();

        return services;
    }
}