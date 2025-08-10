using Projects;
using Raijin.AppHost;
using Raijin.Constants;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<ContainerResource> cryptominisat = builder.AddDockerfile(AppHostDefaults.Cryptominisat.Name,
        AppHostDefaults.Cryptominisat.ContextPath)
    .WithContainerName(AppHostDefaults.Cryptominisat.ContainerName)
    .WithBindMount(AppHostDefaults.Cryptominisat.FileExchangeLocalPath,
        AppHostDefaults.Cryptominisat.FileExchangeContainerPath, isReadOnly: true)
    .WithLifetime(ContainerLifetime.Persistent);

IResourceBuilder<ProjectResource> problemSolvingService = builder.AddProject<Raijin_ProblemSolvingService_Api>(
        AppHostDefaults.ProblemSolvingService.Name)
    .WithHttpHealthCheck(AppHostDefaults.ProblemSolvingService.HealthCheckPath)
    .WithExternalHttpEndpoints()
    .WaitFor(cryptominisat)
    .WithEnvironment(EnvironmentVariables.Cryptominisat.ContainerName,
        AppHostDefaults.Cryptominisat.ContainerName)
    .WithEnvironment(EnvironmentVariables.Cryptominisat.FileExchangeContainerPath,
        AppHostDefaults.Cryptominisat.FileExchangeContainerPath)
    .WithEnvironment(EnvironmentVariables.Cryptominisat.FileExchangeLocalPath,
        AppHostDefaults.Cryptominisat.FileExchangeLocalPath)
    .WithEnvironment(EnvironmentVariables.Cryptominisat.TimeoutSeconds,
        AppHostDefaults.Cryptominisat.TimeoutSeconds);

builder.AddNpmApp("angular-spa", "../Spa")
    .WithReference(problemSolvingService)
    .WaitFor(problemSolvingService)
    .WithHttpEndpoint(env: "Port")
    .PublishAsDockerFile();

builder.AddExecutable(
        "unit-tests",
        "dotnet",
        "../../",
        "test", "--filter", "Category=Unit")
    .WaitFor(problemSolvingService)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Test");

builder.AddExecutable(
    "integration-tests",
    "dotnet",
    "../../",
    "test", "--filter", "Category=Integration")
    .WaitFor(problemSolvingService)
    .WithReference(problemSolvingService)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Test")
    .WithEnvironment(EnvironmentVariables.Cryptominisat.ContainerName,
        AppHostDefaults.Cryptominisat.ContainerName)
    .WithEnvironment(EnvironmentVariables.Cryptominisat.FileExchangeContainerPath,
        AppHostDefaults.Cryptominisat.FileExchangeContainerPath)
    .WithEnvironment(EnvironmentVariables.Cryptominisat.FileExchangeLocalPath,
        AppHostDefaults.Cryptominisat.FileExchangeLocalPath)
    .WithEnvironment(EnvironmentVariables.Cryptominisat.TimeoutSeconds,
        AppHostDefaults.Cryptominisat.TimeoutSeconds);

builder.Build().Run();
