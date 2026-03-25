using Aspire.Hosting.JavaScript;
using Microsoft.Extensions.Hosting;
using Projects;
using Scalar.Aspire;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<RabbitMQServerResource> rabbitMq = builder
    .AddRabbitMQ("rabbit-mq")
    .WithLifetime(ContainerLifetime.Persistent)
    .PublishAsContainer();

IResourceBuilder<PostgresServerResource> applicationDbServer = builder
    .AddPostgres("raijin-db-server")
    .WithLifetime(ContainerLifetime.Persistent)
    //.WithDataVolume("raijin-db-data")
    .PublishAsContainer();

IResourceBuilder<PostgresDatabaseResource> satSolverDb = applicationDbServer
    .AddDatabase("sat-solver-db");

IResourceBuilder<ProjectResource> satSolverMigrationWorker = builder
    .AddProject<Raijin_SatSolver_MigrationWorker>("sat-solver-migration-worker")
    .WithReference(satSolverDb, "sat-solver-db")
    .WaitFor(satSolverDb);

switch (builder.Environment.IsDevelopment())
{
    case true:
        builder.AddProject<Raijin_SatSolver_Worker>("sat-solver-worker")
            .WithReplicas(3)
            .WithEnvironment("CRYPTOMINISAT_FILE_NAME", "cryptominisat5.exe")
            .WithReference(satSolverDb, "sat-solver-db")
            .WaitFor(satSolverDb)
            .WaitFor(satSolverMigrationWorker)
            .WithReference(rabbitMq)
            .WaitFor(rabbitMq);
        break;
    case false:
        for (var i = 0; i < 3; i++)
            builder.AddDockerfile($"sat-solver-worker-{i}", "..", "./SatSolver/Worker/Dockerfile")
                .WithLifetime(ContainerLifetime.Session)
                .PublishAsContainer()
                .WithReference(satSolverDb, "sat-solver-db")
                .WaitFor(satSolverDb)
                .WaitFor(satSolverMigrationWorker)
                .WithReference(rabbitMq)
                .WaitFor(rabbitMq);
        break;
}

IResourceBuilder<ProjectResource> satSolverApi = builder
    .AddProject<Raijin_SatSolver_Api>("sat-solver-api")
    .WithReference(satSolverDb, "sat-solver-db")
    .WaitFor(satSolverDb)
    .WaitForCompletion(satSolverMigrationWorker)
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq)
    .PublishAsDockerFile();

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

IResourceBuilder<PostgresDatabaseResource> queryServiceDb = applicationDbServer
    .AddDatabase("query-service-db");

IResourceBuilder<RedisResource> queryServiceRedis = builder
    .AddRedis("query-service-redis-db")
    .WithLifetime(ContainerLifetime.Persistent)
    .PublishAsContainer();

IResourceBuilder<ProjectResource> queryServiceMigrationWorker = builder
    .AddProject<Raijin_QueryService_MigrationWorker>("query-service-migration-worker")
    .WithReference(queryServiceDb)
    .WaitFor(queryServiceDb);

IResourceBuilder<ProjectResource> queryServiceApi = builder
    .AddProject<Raijin_QueryService_Api>("query-service-api")
    .WithHttpHealthCheck("/health")
    .WithReference(queryServiceDb)
    .WaitFor(queryServiceDb)
    .WaitForCompletion(queryServiceMigrationWorker)
    .WithReference(queryServiceRedis)
    .WaitFor(queryServiceRedis)
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
    .WithReference(satSolverApi)
    .WaitFor(satSolverApi)
    .WithReference(queryServiceApi)
    .WaitFor(queryServiceApi)
    .PublishAsDockerFile();

IResourceBuilder<JavaScriptAppResource> spaFrontend = builder
    .AddJavaScriptApp("spa-frontend", "../Spa", OperatingSystem.IsLinux() ? "start" : "win:start")
    .WithReference(apiGateway)
    .WaitFor(apiGateway)
    .WithHttpEndpoint(env: "PORT")
    .PublishAsDockerFile();

if (builder.Environment.IsDevelopment())
{
    rabbitMq.WithManagementPlugin();
    applicationDbServer.WithPgWeb();


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
        .WithApiReference(satSolverApi, (options, _) =>
        {
            options.AddDocument("v1");
            return Task.CompletedTask;
        })
        .WaitFor(satSolverApi)
        .WithApiReference(queryServiceApi, (options, _) =>
        {
            options.AddDocument("v1");
            return Task.CompletedTask;
        })
        .WaitFor(queryServiceApi)
        .WithApiReference(apiGateway, (options, _) =>
        {
            options.AddDocument("v1");
            return Task.CompletedTask;
        })
        .WaitFor(apiGateway)
        .WithLifetime(ContainerLifetime.Persistent)
        .PublishAsContainer();
}


builder.Build().Run();