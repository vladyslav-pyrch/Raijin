using Aspire.Hosting.JavaScript;
using Projects;
using Scalar.Aspire;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresServerResource> applicationDbServer = builder
    .AddPostgres("raijin-db-server")
    .WithLifetime(ContainerLifetime.Persistent)
    //.WithDataVolume("raijin-db-data")
    .WithPgWeb()
    .PublishAsContainer();

IResourceBuilder<PostgresDatabaseResource> identityServiceDb = applicationDbServer
    .AddDatabase("identity-service-db");

IResourceBuilder<ProjectResource> identityServiceMigrationWorker = builder
    .AddProject<Raijin_IdentityService_MigrationWorker>("identity-service-migration-worker")
    .WithReference(identityServiceDb)
    .WaitFor(identityServiceDb);

IResourceBuilder<ProjectResource> identityServiceApi = builder
    .AddProject<Raijin_IdentityService_Api>("identity-service-api")
    .WithHttpHealthCheck("/health")
    .WithReference(identityServiceDb)
    .WaitFor(identityServiceDb)
    .WaitForCompletion(identityServiceMigrationWorker)
    .PublishAsDockerFile();

IResourceBuilder<PostgresDatabaseResource> combinatoricsServiceDb = applicationDbServer
    .AddDatabase("combinatorics-service-db");

IResourceBuilder<ProjectResource> combinatoricsServiceMigrationWorker = builder
    .AddProject<Raijin_CombinatoricsService_MigrationWorker>("combinatorics-service-migration-worker")
    .WithReference(combinatoricsServiceDb)
    .WaitFor(combinatoricsServiceDb);

IResourceBuilder<ProjectResource> satSolverWorker = builder
    .AddProject<Raijin_CombinatoricsService_SatSolver>("combinatorics-service-sat-solver")
    .WithEnvironment("CRYPTOMINISAT_FILE_NAME", "cryptominisat5.exe")
    .WithReference(combinatoricsServiceDb)
    .WaitFor(combinatoricsServiceDb)
    .WaitForCompletion(combinatoricsServiceMigrationWorker)
    .PublishAsDockerFile();

IResourceBuilder<ProjectResource> combinatoricsServiceApi = builder
    .AddProject<Raijin_CombinatoricsService_Api>("combinatorics-service-api")
    .WithHttpHealthCheck("/health")
    .WithReference(combinatoricsServiceDb)
    .WaitFor(combinatoricsServiceDb)
    .WaitForCompletion(combinatoricsServiceMigrationWorker)
    .PublishAsDockerFile();

IResourceBuilder<ProjectResource> apiGateway = builder
    .AddProject<Raijin_ApiGateway>("api-gateway")
    .WithHttpHealthCheck("/health")
    .WithReference(identityServiceApi)
    .WaitFor(identityServiceApi)
    .WithReference(combinatoricsServiceApi)
    .WaitFor(combinatoricsServiceApi)
    .PublishAsDockerFile();

IResourceBuilder<JavaScriptAppResource> spaFrontend = builder
    .AddViteApp("spa-frontend", "../Spa")
    .WithReference(apiGateway)
    .WaitFor(apiGateway)
    .PublishAsDockerFile();

IResourceBuilder<ScalarResource> scalar = builder
    .AddScalarApiReference(options => options.AllowSelfSignedCertificates = true)
    .WithApiReference(identityServiceApi, (options, _) =>
    {
        options.AddDocument("v1");
        return Task.CompletedTask;
    })
    .WaitFor(identityServiceApi)
    .WithApiReference(combinatoricsServiceApi, (options, _) =>
    {
        options.AddDocument("v1");
        return Task.CompletedTask;
    })
    .WaitFor(combinatoricsServiceApi)
    .WithApiReference(apiGateway, (options, _) =>
    {
        options.AddDocument("v1");
        return Task.CompletedTask;
    })
    .WaitFor(apiGateway)
    .WithLifetime(ContainerLifetime.Persistent)
    .PublishAsContainer();

builder.Build().Run();