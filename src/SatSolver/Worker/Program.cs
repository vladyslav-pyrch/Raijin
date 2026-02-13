using Raijin.SatSolver.Infrastructure;
using Raijin.SatSolver.Worker;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddInfrastructure();

IHost host = builder.Build();
host.Run();
