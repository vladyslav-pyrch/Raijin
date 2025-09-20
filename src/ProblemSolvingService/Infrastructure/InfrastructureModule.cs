using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raijin.Constants;
using Raijin.ProblemSolvingService.Application.Cqrs;
using Raijin.ProblemSolvingService.Application.Features;
using Raijin.ProblemSolvingService.Infrastructure.Cqrs;
using Raijin.ProblemSolvingService.Infrastructure.CryptoMiniSat;

namespace Raijin.ProblemSolvingService.Infrastructure;

public static class InfrastructureModule
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddCqrs();
        services.AddCryptoMiniSat();

        return services;
    }

    private static IServiceCollection AddCqrs(this IServiceCollection services) =>
        services.AddScoped<ISender, DotNetDiSender>();

    private static IServiceCollection AddCryptoMiniSat(this IServiceCollection services)
    {
        services.AddScoped<ISatSolver, CryptominisatSatSolver>();
        services.AddScoped<Cryptominisat>();
        services.AddOptions<CryptominisatOptions>()
            .Configure((CryptominisatOptions options, IConfiguration configuration) =>
            {
                options.ContainerName = configuration[EnvironmentVariables.Cryptominisat.ContainerName]!;
                options.FileExchangeContainerPath =
                    configuration[EnvironmentVariables.Cryptominisat.FileExchangeContainerPath]!;
                options.FileExchangeLocalPath =
                    configuration[EnvironmentVariables.Cryptominisat.FileExchangeLocalPath]!;
                options.TimeoutSeconds =
                    Convert.ToInt32(configuration[EnvironmentVariables.Cryptominisat.TimeoutSeconds]);
            })
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ContainerName),
                "Cryptominisat container name is required."
            ).Validate(
                options => !string.IsNullOrWhiteSpace(options.FileExchangeContainerPath),
                "Cryptominisat container file exchange path is required."
            ).Validate(
                options => !string.IsNullOrWhiteSpace(options.FileExchangeLocalPath),
                "Cryptominisat local file exchange path is required."
            ).Validate(
                options => options.TimeoutSeconds >= 0,
                "Cryptominisat timeout may not be negative."
            );

        return services;
    }
}