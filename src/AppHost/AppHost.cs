using Projects;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

const string cryptominisatContainerName = "cryptominisat";
const string cryptominisatRunCommand = "cryptominisat5";

string cryptominisatFileExchange = Path.Combine(Directory.GetCurrentDirectory(), $@"..\file_exchange\{cryptominisatContainerName}");

IResourceBuilder<ContainerResource> cryptominisat = builder.AddDockerfile(cryptominisatContainerName,
        "../cryptominisat")
    .WithContainerName(cryptominisatContainerName)
    .WithBindMount(cryptominisatFileExchange, "/app/cryptominisat/problems", isReadOnly: true)
    .WithLifetime(ContainerLifetime.Persistent);


IResourceBuilder<ProjectResource> problemSolvingServiceApi = builder.AddProject<Raijin_ProblemSolvingService_Api>(
        "problem-solving-service-api")
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints()
    .WaitFor(cryptominisat)
    .WithEnvironment("CRYPTOMINISAT_CONTAINER_NAME", cryptominisatContainerName)
    .WithEnvironment("CRYPTOMINISAT_RUN_COMMAND", cryptominisatRunCommand)
    .WithEnvironment("CRYPTOMINISAT_FILE_EXCHANGE", cryptominisatFileExchange)
    .WithEnvironment("CRYPTOMINISAT_WORKING_DIRECTORY", "/app/cryptominisat/problems");


IResourceBuilder<NodeAppResource> angularSpa = builder.AddNpmApp("angular-spa", "../Spa")
    .WithReference(problemSolvingServiceApi)
    .WaitFor(problemSolvingServiceApi)
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

IResourceBuilder<ExecutableResource> tests = builder.AddExecutable(
    "tests",
    "dotnet",
    "../../",
    "test",
    "").WaitFor(problemSolvingServiceApi)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Test")
    .WithEnvironment("CRYPTOMINISAT_CONTAINER_NAME", cryptominisatContainerName)
    .WithEnvironment("CRYPTOMINISAT_RUN_COMMAND", cryptominisatRunCommand)
    .WithEnvironment("CRYPTOMINISAT_FILE_EXCHANGE", cryptominisatFileExchange);

builder.Build().Run();
