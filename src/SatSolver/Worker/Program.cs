using Microsoft.EntityFrameworkCore;
using Raijin.SatSolver.Application;
using Raijin.SatSolver.Infrastructure;
using Raijin.SatSolver.Infrastructure.Persistence;
using Raijin.SatSolver.Worker;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddInfrastructure();
builder.Services.AddApplication();

IHost host = builder.Build();

await using (ServiceProvider provide = builder.Services.BuildServiceProvider())
{
    using IServiceScope scope = provide.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var satSolverDbContext = scope.ServiceProvider.GetRequiredService<SatSolverDbContext>();

    logger.LogInformation("Applying database migrations...");

    await satSolverDbContext.Database.MigrateAsync();

    logger.LogInformation("Database migrations applied successfully.");
}

host.Run();
