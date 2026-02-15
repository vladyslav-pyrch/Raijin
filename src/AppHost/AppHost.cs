using Aspire.Hosting.JavaScript;
using Projects;
using Scalar.Aspire;
using static Raijin.AppHost.AppHostDefaults;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<RabbitMQServerResource> rabbitMq = builder.AddRabbitMQ(RabbitMq.Name)
    .WithLifetime(ContainerLifetime.Persistent)
    .PublishAsContainer();

IResourceBuilder<PostgresServerResource> satSolverDbServer = builder
    .AddPostgres(SatSolver.DatabaseServer.Name)
    .WithLifetime(ContainerLifetime.Persistent)
    .PublishAsContainer();

IResourceBuilder<PostgresDatabaseResource> satSolverDb = satSolverDbServer
    .AddDatabase(SatSolver.Database.Name);

IResourceBuilder<ProjectResource> satSolverDomainTest = builder
    .AddProject<Raijin_SatSolver_Domain_Tests>(SatSolver.DomainTests.Name)
    .PublishAsDockerFile();

IResourceBuilder<ProjectResource> satSolverApplicationTest = builder
    .AddProject<Raijin_SatSolver_Application_Tests>(SatSolver.ApplicationTests.Name)
    .PublishAsDockerFile();

IResourceBuilder<ContainerResource> satSolver = builder
    .AddDockerfile(SatSolver.Name, SatSolver.ContextPath, SatSolver.DockerfilePath)
    .WithChildRelationship(satSolverDomainTest)
    .WithChildRelationship(satSolverApplicationTest)
    .WithChildRelationship(satSolverDbServer)
    .WithReference(satSolverDb)
    .WaitFor(satSolverDb)
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq)
    .WithLifetime(ContainerLifetime.Session)
    .PublishAsContainer();

IResourceBuilder<PostgresServerResource> identityServiceDbServer = builder
    .AddPostgres(IdentityService.DatabaseServer.Name)
    .WithLifetime(ContainerLifetime.Persistent)
    .PublishAsContainer();

IResourceBuilder<PostgresDatabaseResource> identityServiceDb =
    identityServiceDbServer.AddDatabase(IdentityService.Database.Name);

IResourceBuilder<ProjectResource> identityService = builder.AddProject<Raijin_IdentityService_Api>(IdentityService.Name)
    .WithHttpHealthCheck(IdentityService.HealthCheckPath)
    .WithChildRelationship(identityServiceDbServer)
    .WithReference(identityServiceDb)
    .WaitFor(identityServiceDb)
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq)
    .PublishAsDockerFile();

IResourceBuilder<PostgresServerResource> combinatoricsServiceDbServer =
    builder.AddPostgres(CombinatoricsService.DatabaseServer.Name)
        .WithLifetime(ContainerLifetime.Persistent)
        .PublishAsContainer();

IResourceBuilder<PostgresDatabaseResource> combinatoricsServiceDb =
    combinatoricsServiceDbServer.AddDatabase(CombinatoricsService.Database.Name);

IResourceBuilder<ProjectResource> combinatoricsService = builder.AddProject<Raijin_CombinatoricsService_Api>(
        CombinatoricsService.Name)
    .WithHttpHealthCheck(CombinatoricsService.HealthCheckPath)
    .WithChildRelationship(combinatoricsServiceDbServer)
    .WithReference(combinatoricsServiceDb)
    .WaitFor(combinatoricsServiceDb)
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq)
    .PublishAsDockerFile();

IResourceBuilder<PostgresServerResource> queryServiceDbServer = builder.AddPostgres(QueryService.DatabaseServer.Name)
    .WithLifetime(ContainerLifetime.Persistent)
    .PublishAsContainer();

IResourceBuilder<PostgresDatabaseResource> queryServiceDb =
    queryServiceDbServer.AddDatabase(QueryService.Database.Name);

IResourceBuilder<RedisResource> queryServiceRedis = builder.AddRedis(QueryService.Redis.Name)
    .WithLifetime(ContainerLifetime.Persistent)
    .PublishAsContainer();

IResourceBuilder<ProjectResource> queryService = builder.AddProject<Raijin_QueryService_Api>(QueryService.Name)
    .WithHttpHealthCheck(QueryService.HealthCheckPath)
    .WithChildRelationship(queryServiceDbServer)
    .WithReference(queryServiceDb)
    .WaitFor(queryServiceDb)
    .WithChildRelationship(queryServiceRedis)
    .WithReference(queryServiceRedis)
    .WaitFor(queryServiceRedis)
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq)
    .PublishAsDockerFile();

IResourceBuilder<ProjectResource> apiGateway = builder.AddProject<Raijin_ApiGateway>(ApiGateway.Name)
    .WithHttpHealthCheck(ApiGateway.HealthCheckPath)
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
    .AddJavaScriptApp(SpaFrontend.Name, SpaFrontend.AppDirectory, SpaFrontend.RunScriptName)
    .WithReference(apiGateway)
    .WaitFor(apiGateway)
    .WithHttpEndpoint(env: "PORT")
    .PublishAsDockerFile();

builder.Build().Run();