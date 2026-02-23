using Aspire.Hosting.JavaScript;
using Projects;
using Scalar.Aspire;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<RabbitMQServerResource> rabbitMq = builder
    .AddRabbitMQ("rabbit-mq")
    .WithLifetime(ContainerLifetime.Persistent)
    .PublishAsContainer();

IResourceBuilder<PostgresServerResource> satSolverDbServer = builder
    .AddPostgres("sat-solver-db-server")
    .WithLifetime(ContainerLifetime.Persistent)
    .PublishAsContainer();

IResourceBuilder<PostgresDatabaseResource> satSolverDb = satSolverDbServer
    .AddDatabase("sat-solver-db");

IResourceBuilder<ProjectResource> satSolverDomainTest = builder
    .AddProject<Raijin_SatSolver_Domain_Tests>("sat-solver-domain-tests")
    .PublishAsDockerFile();

IResourceBuilder<ProjectResource> satSolverApplicationTest = builder
    .AddProject<Raijin_SatSolver_Application_Tests>("sat-solver-application-tests")
    .PublishAsDockerFile();

IResourceBuilder<ContainerResource> satSolverWorker = builder
    .AddDockerfile("sat-solver-worker", "..", "./SatSolver/Worker/Dockerfile")
    .WithChildRelationship(satSolverDomainTest)
    .WithChildRelationship(satSolverApplicationTest)
    .WithChildRelationship(satSolverDbServer)
    .WithReference(satSolverDb)
    .WaitFor(satSolverDb)
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq)
    .WithEnvironment("RABBIT_MQ__EXCHANGE", "app.events")
    .WithLifetime(ContainerLifetime.Persistent)
    .PublishAsContainer();

IResourceBuilder<ProjectResource> satSolverApi = builder
    .AddProject<Raijin_SatSolver_Api>("sat-solver-api")
    .WithChildRelationship(satSolverDomainTest)
    .WithChildRelationship(satSolverApplicationTest)
    .WithChildRelationship(satSolverDbServer)
    .WithReference(satSolverDb)
    .WaitFor(satSolverDb)
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq)
    .WithEnvironment("RABBIT_MQ__EXCHANGE", "app.events")
    .PublishAsDockerFile();

IResourceBuilder<PostgresServerResource> identityServiceDbServer = builder
    .AddPostgres("identity-service-db-server")
    .WithLifetime(ContainerLifetime.Persistent)
    .PublishAsContainer();

IResourceBuilder<PostgresDatabaseResource> identityServiceDb =
    identityServiceDbServer.AddDatabase("identity-service-db");

IResourceBuilder<ProjectResource> identityServiceApi = builder
    .AddProject<Raijin_IdentityService_Api>("identity-service-api")
    .WithHttpHealthCheck("/health")
    .WithChildRelationship(identityServiceDbServer)
    .WithReference(identityServiceDb)
    .WaitFor(identityServiceDb)
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq)
    .PublishAsDockerFile();

IResourceBuilder<PostgresServerResource> combinatoricsServiceDbServer = builder
    .AddPostgres("combinatorics-service-db-server")
    .WithLifetime(ContainerLifetime.Persistent)
    .PublishAsContainer();

IResourceBuilder<PostgresDatabaseResource> combinatoricsServiceDb = combinatoricsServiceDbServer
    .AddDatabase("combinatorics-service-db");

IResourceBuilder<ProjectResource> combinatoricsServiceApi = builder
    .AddProject<Raijin_CombinatoricsService_Api>("combinatorics-service-api")
    .WithHttpHealthCheck("/health")
    .WithChildRelationship(combinatoricsServiceDbServer)
    .WithReference(combinatoricsServiceDb)
    .WaitFor(combinatoricsServiceDb)
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq)
    .PublishAsDockerFile();

IResourceBuilder<PostgresServerResource> queryServiceDbServer = builder
    .AddPostgres("query-service-db-server")
    .WithLifetime(ContainerLifetime.Persistent)
    .PublishAsContainer();

IResourceBuilder<PostgresDatabaseResource> queryServiceDb =
    queryServiceDbServer.AddDatabase("query-service-db");

IResourceBuilder<RedisResource> queryServiceRedis = builder
    .AddRedis("query-service-redis-db")
    .WithLifetime(ContainerLifetime.Persistent)
    .PublishAsContainer();

IResourceBuilder<ProjectResource> queryServiceApi = builder
    .AddProject<Raijin_QueryService_Api>("query-service-api")
    .WithHttpHealthCheck("/health")
    .WithChildRelationship(queryServiceDbServer)
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

IResourceBuilder<ScalarResource> scalar = builder.AddScalarApiReference()
    .WithApiReference(identityServiceApi)
    .WaitFor(identityServiceApi)
    .WithApiReference(combinatoricsServiceApi)
    .WaitFor(combinatoricsServiceApi)
    .WithReference(satSolverApi)
    .WaitFor(satSolverApi)
    .WithApiReference(queryServiceApi)
    .WaitFor(queryServiceApi)
    .WithApiReference(apiGateway)
    .WaitFor(apiGateway)
    .WithLifetime(ContainerLifetime.Persistent)
    .PublishAsContainer();

IResourceBuilder<JavaScriptAppResource> spaFrontend = builder
    .AddJavaScriptApp("spa-frontend", "../Spa", OperatingSystem.IsLinux() ? "start" : "win:start")
    .WithReference(apiGateway)
    .WaitFor(apiGateway)
    .WithHttpEndpoint(env: "PORT")
    .PublishAsDockerFile();

builder.Build().Run();