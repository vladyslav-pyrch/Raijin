using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Quartz;
using Raijin.CombinatoricsService.Application.Messaging;
using Raijin.CombinatoricsService.Application.Persistence;
using Raijin.CombinatoricsService.Application.Solvers;
using Raijin.CombinatoricsService.Infrastructure.Messaging;
using Raijin.CombinatoricsService.Infrastructure.Persistence;
using Raijin.CombinatoricsService.Infrastructure.Persistence.Repositories;
using Raijin.CombinatoricsService.Infrastructure.Solvers.Cadical;
using Raijin.CombinatoricsService.Infrastructure.Solvers.Cryptominisat;

namespace Raijin.CombinatoricsService.Infrastructure;

public static class InfrastructureModule
{
    public static Assembly Assembly => typeof(InfrastructureModule).Assembly;

    public static void AddInfrastructure(this IServiceCollection services,
        Action<IServiceCollectionQuartzConfigurator>? quartzConfiguration = null)
    {
        services.AddMessaging();
        services.AddPersistence();
        services.AddSolvers();
        services.AddQuartz(quartzConfiguration);
    }

    private static void AddMessaging(this IServiceCollection services)
    {
        services.AddScoped<IMediator, ServiceProviderMediator>();
    }

    public static void AddPersistence(this IServiceCollection services)
    {
        services.AddDbContextPool<CombinatoricsServiceDbContext>((provider, builder) =>
        {
            NpgsqlDataSource dataSource = new NpgsqlDataSourceBuilder(GetDatabaseConnectionString(provider))
                .Build();

            builder.UseNpgsql(dataSource, optionsBuilder => optionsBuilder.MigrationsAssembly(Assembly));
        });
        services.AddScoped<IUnitOfWork, CombinatoricsServiceUnitOfWork>();
        services.AddScoped<IProblemRepository, ProblemRepository>();
    }

    private static void AddSolvers(this IServiceCollection services)
    {
        services.AddOptions<CryptominisatSolveOptions>()
            .Configure((CryptominisatSolveOptions options, IConfiguration configuration) =>
            {
                IConfigurationSection section = configuration.GetSection(CryptominisatSolveOptions.SectionName);
                if (section[nameof(CryptominisatSolveOptions.FileName)] is { } fileName)
                    options.FileName = fileName;
                if (int.TryParse(section[nameof(CryptominisatSolveOptions.TimeoutSeconds)], out int timeout))
                    options.TimeoutSeconds = timeout;
                if (section[nameof(CryptominisatSolveOptions.CnfDirectory)] is { } dir)
                    options.CnfDirectory = dir;
            });
        services.AddTransient<ICryptominisatCli, CryptominisatCli>();
        services.AddTransient<ISatSolver, CryptominisatSolver>();

        services.AddOptions<CadicalSolveOptions>()
            .Configure((CadicalSolveOptions options, IConfiguration configuration) =>
            {
                IConfigurationSection section = configuration.GetSection(CadicalSolveOptions.SectionName);
                if (section[nameof(CadicalSolveOptions.FileName)] is { } fileName)
                    options.FileName = fileName;
                if (int.TryParse(section[nameof(CadicalSolveOptions.TimeoutSeconds)], out int timeout))
                    options.TimeoutSeconds = timeout;
                if (section[nameof(CadicalSolveOptions.CnfDirectory)] is { } dir)
                    options.CnfDirectory = dir;
            });
        services.AddTransient<ICadicalCli, CadicalCli>();
        services.AddTransient<ISatSolver, CadicalSolver>();
    }

    private static string GetDatabaseConnectionString(IServiceProvider provider) =>
        provider.GetRequiredService<IConfiguration>().GetConnectionString("combinatorics-service-db")
        ?? throw new InvalidOperationException("Database connection string is not configured.");
}