using Aspire.Hosting.JavaScript;
using Projects;
using Scalar.Aspire;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);


IResourceBuilder<RabbitMQServerResource> rabbitMq = builder
    .AddRabbitMQ("rabbit-mq")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithManagementPlugin()
    .PublishAsContainer();

IResourceBuilder<PostgresServerResource> applicationDbServer = builder
    .AddPostgres("raijin-db-server")
    .WithLifetime(ContainerLifetime.Persistent)
    //.WithDataVolume("raijin-db-data")
    .WithPgWeb()
    .PublishAsContainer();

IResourceBuilder<PostgresDatabaseResource> satSolverDb = applicationDbServer
    .AddDatabase("sat-solver-db");

IResourceBuilder<ProjectResource> satSolverMigrationWorker = builder
    .AddProject<Raijin_SatSolver_MigrationWorker>("sat-solver-migration-worker")
    .WithReference(satSolverDb, "sat-solver-db")
    .WaitFor(satSolverDb);

IResourceBuilder<ProjectResource> satSolverEventConsumerWorker = builder
    .AddProject<Raijin_SatSolver_EventConsumerWorker>("sat-solver-event-consumer")
    .WithReference(satSolverDb, "sat-solver-db")
    .WaitFor(satSolverDb)
    .WaitForCompletion(satSolverMigrationWorker)
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq);

IResourceBuilder<ProjectResource> satSolverWorker = builder.AddProject<Raijin_SatSolver_Worker>("sat-solver-worker")
    .WithEnvironment("CRYPTOMINISAT_FILE_NAME", "cryptominisat5.exe")
    .WithReference(satSolverDb, "sat-solver-db")
    .WaitFor(satSolverDb)
    .WaitForCompletion(satSolverMigrationWorker)
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq);

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
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq)
    .PublishAsDockerFile();

IResourceBuilder<PostgresDatabaseResource> combinatoricsServiceDb = applicationDbServer
    .AddDatabase("combinatorics-service-db");

IResourceBuilder<ProjectResource> combinatoricsServiceMigrationWorker = builder
    .AddProject<Raijin_CombinatoricsService_MigrationWorker>("combinatorics-service-migration-worker")
    .WithReference(combinatoricsServiceDb)
    .WaitFor(combinatoricsServiceDb);

IResourceBuilder<ProjectResource> combinatoricsServiceApi = builder
    .AddProject<Raijin_CombinatoricsService_Api>("combinatorics-service-api")
    .WithHttpHealthCheck("/health")
    .WithReference(combinatoricsServiceDb)
    .WaitFor(combinatoricsServiceDb)
    .WaitForCompletion(combinatoricsServiceMigrationWorker)
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq)
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