using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Raijin.SatSolver.Application.Abstractions;
using Raijin.SatSolver.Infrastructure.Solver;

namespace Raijin.SatSolver.Infrastructure;

public static class Module
{
    public static Assembly Assembly => typeof(Module).Assembly;

    public static void AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ISatSolver, CryptominisatSolver>();
    }
}
