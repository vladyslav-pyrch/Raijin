using Raijin.SatSolver.Application;
using Raijin.SatSolver.Infrastructure;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructureWorker();
builder.Services.AddApplication();

IHost host = builder.Build();

await host.ApplyMigrations();

await host.RunAsync();
