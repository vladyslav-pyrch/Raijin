using OpenTelemetry.Trace;
using Quartz;
using Raijin.CombinatoricsService.Application;
using Raijin.CombinatoricsService.Infrastructure;
using Raijin.CombinatoricsService.SatSolver.Jobs;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddInfrastructure(configurator =>
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