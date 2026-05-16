using Raijin.CombinatoricsService.Application;
using Raijin.CombinatoricsService.Infrastructure;
using Raijin.CombinatoricsService.MigrationWorker;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.AddPersistence();
builder.Services.AddConverters();
builder.Services.AddParsers();
builder.Services.AddHostedService<MigrationWorker>();
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(MigrationWorker.ActivitySourceName));

IHost host = builder.Build();
host.LogRaijinStartup();
host.Run();
