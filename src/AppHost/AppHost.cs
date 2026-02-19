using Aspire.Hosting.JavaScript;
using Projects;
using Scalar.Aspire;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<RabbitMQServerResource> rabbitMq = builder.AddRabbitMQ("rabbit-mq")
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

IResourceBuilder<ContainerResource> satSolver = builder
    .AddDockerfile("sat-solver", "../SatSolver", "./Worker/Dockerfile")
    .WithChildRelationship(satSolverDomainTest)
    .WithChildRelationship(satSolverApplicationTest)
    .WithChildRelationship(satSolverDbServer)
    .WithReference(satSolverDb)
    .WaitFor(satSolverDb)
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq)
    .WithEnvironment("RABBIT_MQ__EXCHANGE", "sat-solver.exchange")
    .WithLifetime(ContainerLifetime.Session)
    .PublishAsContainer();

IResourceBuilder<PostgresServerResource> identityServiceDbServer = builder
    .AddPostgres("identity-service-db-server")
    .WithLifetime(ContainerLifetime.Persistent)
    .PublishAsContainer();

IResourceBuilder<PostgresDatabaseResource> identityServiceDb =
    identityServiceDbServer.AddDatabase("identity-service-db");

IResourceBuilder<ProjectResource> identityService = builder.AddProject<Raijin_IdentityService_Api>("identity-service")
    .WithHttpHealthCheck("/health")
    .WithChildRelationship(identityServiceDbServer)
    .WithReference(identityServiceDb)
    .WaitFor(identityServiceDb)
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq)
    .PublishAsDockerFile();

IResourceBuilder<PostgresServerResource> combinatoricsServiceDbServer =
    builder.AddPostgres("combinatorics-service-db-server")
        .WithLifetime(ContainerLifetime.Persistent)
        .PublishAsContainer();

IResourceBuilder<PostgresDatabaseResource> combinatoricsServiceDb =
    combinatoricsServiceDbServer.AddDatabase("combinatorics-service-db");

IResourceBuilder<ProjectResource> combinatoricsService = builder.AddProject<Raijin_CombinatoricsService_Api>(
        "combinatorics-service")
    .WithHttpHealthCheck("/health")
    .WithChildRelationship(combinatoricsServiceDbServer)
    .WithReference(combinatoricsServiceDb)
    .WaitFor(combinatoricsServiceDb)
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq)
    .PublishAsDockerFile();

IResourceBuilder<PostgresServerResource> queryServiceDbServer = builder.AddPostgres("query-service-db-server")
    .WithLifetime(ContainerLifetime.Persistent)
    .PublishAsContainer();

IResourceBuilder<PostgresDatabaseResource> queryServiceDb =
    queryServiceDbServer.AddDatabase("query-service-db");

IResourceBuilder<RedisResource> queryServiceRedis = builder.AddRedis("query-service-redis-db")
    .WithLifetime(ContainerLifetime.Persistent)
    .PublishAsContainer();

IResourceBuilder<ProjectResource> queryService = builder.AddProject<Raijin_QueryService_Api>("query-service")
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

IResourceBuilder<ProjectResource> apiGateway = builder.AddProject<Raijin_ApiGateway>("api-gateway")
    .WithHttpHealthCheck("/health")
    .WithReference(identityService)
    .WaitFor(identityService)
    .WithReference(combinatoricsService)
    .WaitFor(combinatoricsService)
    .WithReference(queryService)
    .WaitFor(queryService)
    .PublishAsDockerFile();

IResourceBuilder<ScalarResource> scalar = builder.AddScalarApiReference()
    .WithApiReference(identityService)
    .WaitFor(identityService)
    .WithApiReference(combinatoricsService)
    .WaitFor(combinatoricsService)
    .WithApiReference(queryService)
    .WaitFor(queryService)
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