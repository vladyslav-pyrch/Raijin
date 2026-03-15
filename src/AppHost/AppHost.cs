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
    .PublishAsContainer();

IResourceBuilder<PostgresDatabaseResource> satSolverWorkerDb = applicationDbServer
    .AddDatabase("sat-solver-worker-db");

IResourceBuilder<ContainerResource> satSolverWorker = builder
    .AddDockerfile("sat-solver-worker", "..", "./SatSolver/Worker/Dockerfile")
    .WithChildRelationship(satSolverWorkerDb)
    .WithReference(satSolverWorkerDb, connectionName: "sat-solver-db")
    .WaitFor(satSolverWorkerDb)
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq)
    .WithLifetime(ContainerLifetime.Session)
    .PublishAsContainer();

IResourceBuilder<PostgresDatabaseResource> satSolverApiDb = applicationDbServer
    .AddDatabase("sat-solver-api-db");

IResourceBuilder<ProjectResource> satSolverApi = builder
    .AddProject<Raijin_SatSolver_Api>("sat-solver-api")
    .WithChildRelationship(satSolverApiDb)
    .WithReference(satSolverApiDb, connectionName: "sat-solver-db")
    .WaitFor(satSolverApiDb)
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq)
    .PublishAsDockerFile();

IResourceBuilder<PostgresDatabaseResource> identityServiceDb = applicationDbServer
    .AddDatabase("identity-service-db");

IResourceBuilder<ProjectResource> identityServiceApi = builder
    .AddProject<Raijin_IdentityService_Api>("identity-service-api")
    .WithHttpHealthCheck("/health")
    .WithChildRelationship(identityServiceDb)
    .WithReference(identityServiceDb)
    .WaitFor(identityServiceDb)
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq)
    .PublishAsDockerFile();

IResourceBuilder<PostgresDatabaseResource> combinatoricsServiceDb = applicationDbServer
    .AddDatabase("combinatorics-service-db");

IResourceBuilder<ProjectResource> combinatoricsServiceApi = builder
    .AddProject<Raijin_CombinatoricsService_Api>("combinatorics-service-api")
    .WithHttpHealthCheck("/health")
    .WithChildRelationship(combinatoricsServiceDb)
    .WithReference(combinatoricsServiceDb)
    .WaitFor(combinatoricsServiceDb)
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq)
    .PublishAsDockerFile();

IResourceBuilder<PostgresDatabaseResource> queryServiceDb = applicationDbServer
    .AddDatabase("query-service-db");

IResourceBuilder<RedisResource> queryServiceRedis = builder
    .AddRedis("query-service-redis-db")
    .WithLifetime(ContainerLifetime.Persistent)
    .PublishAsContainer();

IResourceBuilder<ProjectResource> queryServiceApi = builder
    .AddProject<Raijin_QueryService_Api>("query-service-api")
    .WithHttpHealthCheck("/health")
    .WithChildRelationship(queryServiceDb)
    .WithReference(queryServiceDb)
    .WaitFor(queryServiceDb)
    .WithChildRelationship(queryServiceRedis)
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
        .WithApiReference(identityServiceApi, options => options.AddDocument("v1"))
        .WaitFor(identityServiceApi)
        .WithApiReference(combinatoricsServiceApi, options => options.AddDocument("v1"))
        .WaitFor(combinatoricsServiceApi)
        .WithApiReference(satSolverApi, options => options.AddDocument("v1"))
        .WaitFor(satSolverApi)
        .WithApiReference(queryServiceApi, options => options.AddDocument("v1"))
        .WaitFor(queryServiceApi)
        .WithApiReference(apiGateway, options => options.AddDocument("v1"))
        .WaitFor(apiGateway)
        .WithLifetime(ContainerLifetime.Persistent)
        .PublishAsContainer();
}


builder.Build().Run();