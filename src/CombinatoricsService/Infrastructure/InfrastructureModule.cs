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
using Raijin.CombinatoricsService.Infrastructure.Solvers.Cryptominisat;

namespace Raijin.CombinatoricsService.Infrastructure;

public static class InfrastructureModule
{
    public static Assembly Assembly => typeof(InfrastructureModule).Assembly;

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        Action<IServiceCollectionQuartzConfigurator>? quartzConfiguration = null
    )
    {
        ArgumentNullException.ThrowIfNull(quartzConfiguration);

        services.AddMessaging();
        services.AddPersistence();
        services.AddSolvers();
        services.AddQuartz(quartzConfiguration);

        return services;
    }

    private static IServiceCollection AddMessaging(this IServiceCollection services)
    {
        services.AddScoped<IMediator, ServiceProviderMediator>();

        return services;
    }

    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddDbContextPool<CombinatoricsServiceDbContext>((provider, builder) =>
        {
            NpgsqlDataSource dataSource = new NpgsqlDataSourceBuilder(GetDatabaseConnectionString(provider))
                .Build();

            builder.UseNpgsql(dataSource, optionsBuilder => optionsBuilder.MigrationsAssembly(Assembly));
        });
        services.AddScoped<IUnitOfWork, CombinatoricsServiceUnitOfWork>();
        services.AddScoped<IProblemRepository, ProblemRepository>();
        services.AddScoped<ISatRunRepository, SatRunRepository>();

        return services;
    }

    private static IServiceCollection AddSolvers(this IServiceCollection services)
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

        return services;
    }

    private static string GetDatabaseConnectionString(IServiceProvider provider) =>
        provider.GetRequiredService<IConfiguration>().GetConnectionString("combinatorics-service-db")
        ?? throw new InvalidOperationException("Database connection string is not configured.");
}