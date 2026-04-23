using Aspire.Hosting.JavaScript;
using Projects;
using Scalar.Aspire;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresServerResource> applicationDbServer = builder
    .AddPostgres("raijin-db-server")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("raijin-db-data")
    .WithPgWeb()
    .PublishAsContainer();

IResourceBuilder<PostgresDatabaseResource> combinatoricsServiceDb = applicationDbServer
    .AddDatabase("combinatorics-service-db");

IResourceBuilder<ProjectResource> combinatoricsServiceMigrationWorker = builder
    .AddProject<Raijin_CombinatoricsService_MigrationWorker>("combinatorics-service-migration-worker")
    .WithReference(combinatoricsServiceDb)
    .WaitFor(combinatoricsServiceDb);

IResourceBuilder<ProjectResource> combinatoricsServiceSatSolver = builder
    .AddProject<Raijin_CombinatoricsService_SatSolver>("combinatorics-service-sat-solver")
    .WithEnvironment("MAX_JOBS_COUNT", "3")
    .WithEnvironment("MAX_REFIRE_COUNT", "3")
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

IResourceBuilder<JavaScriptAppResource> spaFrontend = builder
    .AddViteApp("spa-frontend", "../Spa")
    .WithReference(combinatoricsServiceApi)
    .WaitFor(combinatoricsServiceApi)
    .PublishAsDockerFile();

IResourceBuilder<ScalarResource> scalar = builder
    .AddScalarApiReference(options => options.AllowSelfSignedCertificates = true)
    .WithApiReference(combinatoricsServiceApi, (options, _) =>
    {
        options.AddDocument("v1");
        return Task.CompletedTask;
    })
    .WaitFor(combinatoricsServiceApi)
    .WithLifetime(ContainerLifetime.Persistent)
    .PublishAsContainer();

builder.Build().Run();