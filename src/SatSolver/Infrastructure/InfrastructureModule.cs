using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raijin.SatSolver.Application.Abstractions;
using Raijin.SatSolver.Infrastructure.Messaging;
using Raijin.SatSolver.Infrastructure.Persistence;
using Raijin.SatSolver.Infrastructure.Persistence.Repositories;
using Raijin.SatSolver.Infrastructure.Solver;

namespace Raijin.SatSolver.Infrastructure;

public static class InfrastructureModule
{
    public static Assembly Assembly => typeof(InfrastructureModule).Assembly;

    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ISatSolver, CryptominisatSolver>();
        services.AddScoped<IEventBus, RabbitMqEventBus>();
        services.AddScoped<ISatProblemRepository, SatProblemRepository>();
        services.AddDbContextPool<SatSolverDbContext>((provider, builder) =>
        {
            // TODO
            builder.UseNpgsql(provider.GetRequiredService<IConfiguration>().GetConnectionString("sat-solver-db"));
        });

        return services;
    }
}
