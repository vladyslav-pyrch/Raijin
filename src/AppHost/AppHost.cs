using Aspire.Hosting.JavaScript;
using Projects;
using Raijin.Constants;
using static Raijin.AppHost.AppHostDefaults;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<RabbitMQServerResource> rabbitMq = builder.AddRabbitMQ(RabbitMq.Name)
    .WithLifetime(ContainerLifetime.Persistent)
    .PublishAsContainer();

IResourceBuilder<ContainerResource> cryptominisat = builder.AddDockerfile(Cryptominisat.Name, Cryptominisat.ContextPath)
    .WithContainerName(Cryptominisat.ContainerName)
    .WithBindMount(Cryptominisat.MountLocalPath,
        Cryptominisat.MountContainerPath, true)
    .WithLifetime(ContainerLifetime.Persistent)
    .PublishAsContainer();

IResourceBuilder<PostgresServerResource> satSolverDb = builder.AddPostgres(SatSolver.Database.Name)
    .WithLifetime(ContainerLifetime.Persistent)
    .PublishAsContainer();

IResourceBuilder<ProjectResource> satSolver = builder.AddProject<Raijin_SatSolver_Worker>(SatSolver.Name)
    .WaitFor(cryptominisat)
    .WithEnvironment(EnvironmentVariables.Cryptominisat.ContainerName,
        Cryptominisat.ContainerName)
    .WithEnvironment(EnvironmentVariables.Cryptominisat.MountContainerPath,
        Cryptominisat.MountContainerPath)
    .WithEnvironment(EnvironmentVariables.Cryptominisat.MountLocalPath,
        Cryptominisat.MountLocalPath)
    .WithReference(satSolverDb)
    .WaitFor(satSolverDb)
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq)
    .PublishAsDockerFile();

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

IResourceBuilder<JavaScriptAppResource> spaFrontend = builder
    .AddJavaScriptApp(SpaFrontend.Name, SpaFrontend.AppDirectory, SpaFrontend.RunScriptName)
    .WithReference(apiGateway)
    .WaitFor(apiGateway)
    .WithHttpEndpoint(env: "PORT")
    .PublishAsDockerFile();

builder.Build().Run();