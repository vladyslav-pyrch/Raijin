using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var problemSolvingServiceApi = builder.AddProject<Njinx_ProblemSolvingService_Api>(
    "problem-solving-service-api"
).WithHttpHealthCheck("/health")
.WithExternalHttpEndpoints();

builder.AddNpmApp("angular-spa", "../Spa")
    .WithReference(problemSolvingServiceApi)
    .WaitFor(problemSolvingServiceApi)
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();
