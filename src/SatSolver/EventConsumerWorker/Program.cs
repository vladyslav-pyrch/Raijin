using MassTransit;
using Raijin.Application.Contracts;
using Raijin.SatSolver.Application;
using Raijin.SatSolver.EventConsumerWorker.Consumers;
using Raijin.SatSolver.Infrastructure;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddInfrastructure(
    configurator => configurator.AddConsumers(typeof(Program).Assembly),
    busFactoryConfiguration: (context, configurator) =>
    {
        configurator.ReceiveEndpoint(
            context.EndpointNameFormatter.Message<ISubmitSatProblem>(),
            endpoint => endpoint.ConfigureConsumer<SubmitSatProblemConsumer>(context)
        );
    });
builder.Services.AddApplication();

IHost host = builder.Build();

await host.RunAsync();