using OpenTelemetry.Trace;
using Quartz;
using Raijin.SatSolver.Application;
using Raijin.SatSolver.Infrastructure;
using Raijin.SatSolver.Worker.Jobs;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddInfrastructure(serviceCollectionQuartzConfiguration: configurator =>
{
    configurator.AddJob<SolvePendingProblemsJob>(jobConfigurator =>
    {
        jobConfigurator.WithIdentity(SolvePendingProblemsJob.Key);
    });

    configurator.AddTrigger(triggerConfigurator =>
    {
        triggerConfigurator.ForJob(SolvePendingProblemsJob.Key);
        triggerConfigurator.WithSimpleSchedule(simpleScheduleBuilder =>
        {
            simpleScheduleBuilder.RepeatForever();
            simpleScheduleBuilder.WithIntervalInSeconds(5);
        });
    });
});
builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);
builder.Services.AddOpenTelemetry().WithTracing(providerBuilder => providerBuilder.AddQuartzInstrumentation());
builder.Services.AddApplication();

IHost host = builder.Build();

await host.RunAsync();