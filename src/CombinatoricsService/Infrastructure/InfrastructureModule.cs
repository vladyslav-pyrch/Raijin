using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Parsing.DimacsToSat;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Application.Solvers;
using Raijin.CombinatoricsService.Infrastructure.Converters;
using Raijin.CombinatoricsService.Infrastructure.Messaging;
using Raijin.CombinatoricsService.Infrastructure.Persistence;
using Raijin.CombinatoricsService.Infrastructure.Persistence.Repositories;
using Raijin.CombinatoricsService.Infrastructure.Solvers.Cadical;
using Raijin.CombinatoricsService.Infrastructure.Solvers.Cryptominisat;

namespace Raijin.CombinatoricsService.Infrastructure;

public static class InfrastructureModule
{
    public static Assembly Assembly => typeof(InfrastructureModule).Assembly;

    public static void AddInfrastructure(this IHostApplicationBuilder builder,
        Action<IServiceCollectionQuartzConfigurator>? quartzConfiguration = null)
    {
        builder.AddPersistence();
        builder.Services.AddMessaging();
        builder.Services.AddSolvers();
        builder.Services.AddQuartz(quartzConfiguration);
        builder.Services.AddConverters();
    }

    public static void AddConverters(this IServiceCollection services)
    {
        services.AddTransient<BoolExprJsonConverter>();
        services.AddSingleton<DimacsToSatParser>();
    }

    private static void AddMessaging(this IServiceCollection services)
    {
        services.AddScoped<IMediator, ServiceProviderMediator>();
    }

    public static void AddPersistence(this IHostApplicationBuilder builder)
    {
        builder.AddAzureNpgsqlDbContext<CombinatoricsServiceDbContext>("raijin-comb-db", settings => settings.DisableRetry = true);
        builder.Services.AddScoped<IUnitOfWork, CombinatoricsServiceUnitOfWork>();
        builder.Services.AddScoped<IProblemRepository, ProblemRepository>();
    }

    private static void AddSolvers(this IServiceCollection services)
    {
        services.AddOptions<CryptominisatSolveOptions>()
            .Configure((CryptominisatSolveOptions options, IConfiguration configuration) =>
            {
                IConfigurationSection section = configuration.GetSection(CryptominisatSolveOptions.SectionName);
                if (section[nameof(CryptominisatSolveOptions.ExecutableFilePath)] is { } fileName)
                    options.ExecutableFilePath = fileName;
                if (int.TryParse(section[nameof(CryptominisatSolveOptions.TimeoutSeconds)], out int timeout))
                    options.TimeoutSeconds = timeout;
            });
        services.AddTransient<ICryptominisatCli, CryptominisatCli>();
        services.AddTransient<ISatSolver, CryptominisatSolver>();

        services.AddOptions<CadicalSolveOptions>()
            .Configure((CadicalSolveOptions options, IConfiguration configuration) =>
            {
                IConfigurationSection section = configuration.GetSection(CadicalSolveOptions.SectionName);
                if (section[nameof(CadicalSolveOptions.ExecutableFilePath)] is { } fileName)
                    options.ExecutableFilePath = fileName;
                if (int.TryParse(section[nameof(CadicalSolveOptions.TimeoutSeconds)], out int timeout))
                    options.TimeoutSeconds = timeout;
            });
        services.AddTransient<ICadicalCli, CadicalCli>();
        services.AddTransient<ISatSolver, CadicalSolver>();
    }
}
