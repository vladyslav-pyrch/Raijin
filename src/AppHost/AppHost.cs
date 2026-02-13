using Aspire.Hosting.JavaScript;
using Projects;
using Scalar.Aspire;
using static Raijin.AppHost.AppHostDefaults;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<RabbitMQServerResource> rabbitMq = builder.AddRabbitMQ(RabbitMq.Name)
    .WithLifetime(ContainerLifetime.Persistent)
    .PublishAsContainer();

IResourceBuilder<PostgresServerResource> satSolverDb = builder.AddPostgres(SatSolver.Database.Name)
    .WithLifetime(ContainerLifetime.Persistent)
    .PublishAsContainer();

IResourceBuilder<ProjectResource> satSolverDomainTest = builder
    .AddProject<Raijin_SatSolver_Domain_Tests>(SatSolver.DomainTests.Name)
    .PublishAsDockerFile();

for (var i = 0; i < 3; i++) // 3 replicas; Figure out how to make only one of them consume events from the queue.
    builder.AddDockerfile($"{SatSolver.Name}-{i}", SatSolver.ContextPath, SatSolver.DockerfilePath)
        .WaitFor(satSolverDomainTest)
        .WithReference(satSolverDb)
        .WaitFor(satSolverDb)
        .WithReference(rabbitMq)
        .WaitFor(rabbitMq)
        .WithLifetime(ContainerLifetime.Session)
        .PublishAsContainer();

IResourceBuilder<PostgresServerResource> identityServiceDb = builder.AddPostgres(IdentityService.Database.Name)
    .WithLifetime(ContainerLifetime.Persistent)
    .PublishAsContainer();

IResourceBuilder<ProjectResource> identityService = builder.AddProject<Raijin_IdentityService_Api>(IdentityService.Name)
    .WithHttpHealthCheck(IdentityService.HealthCheckPath)
    .WithReference(identityServiceDb)
    .WaitFor(identityServiceDb)
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq)
    .PublishAsDockerFile();

IResourceBuilder<PostgresServerResource> combinatoricsServiceDb =
    builder.AddPostgres(CombinatoricsService.Database.Name)
        .WithLifetime(ContainerLifetime.Persistent)
        .PublishAsContainer();

IResourceBuilder<ProjectResource> combinatoricsService = builder.AddProject<Raijin_CombinatoricsService_Api>(
        CombinatoricsService.Name)
    .WithHttpHealthCheck(CombinatoricsService.HealthCheckPath)
    .WithReference(combinatoricsServiceDb)
    .WaitFor(combinatoricsServiceDb)
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq)
    .PublishAsDockerFile();

IResourceBuilder<PostgresServerResource> queryServiceDb = builder.AddPostgres(QueryService.Database.Name)
    .WithLifetime(ContainerLifetime.Persistent)
    .PublishAsContainer();

IResourceBuilder<RedisResource> queryServiceRedis = builder.AddRedis(QueryService.Redis.Name)
    .WithLifetime(ContainerLifetime.Persistent)
    .PublishAsContainer();

IResourceBuilder<ProjectResource> queryService = builder.AddProject<Raijin_QueryService_Api>(QueryService.Name)
    .WithHttpHealthCheck(QueryService.HealthCheckPath)
    .WithReference(queryServiceDb)
    .WaitFor(queryServiceDb)
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
    .PublishAsContainer();

IResourceBuilder<JavaScriptAppResource> spaFrontend = builder
    .AddJavaScriptApp(SpaFrontend.Name, SpaFrontend.AppDirectory, SpaFrontend.RunScriptName)
    .WithReference(apiGateway)
    .WaitFor(apiGateway)
    .WithHttpEndpoint(env: "PORT")
    .PublishAsDockerFile();

builder.Build().Run();