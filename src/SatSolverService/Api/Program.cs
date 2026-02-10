using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Raijin.Constants;
using Raijin.SatSolverService.Api.Endpoints.V1;
using Raijin.SatSolverService.Api.Endpoints.V1.SatProblems;
using Raijin.SatSolverService.Api.Endpoints.V1.SatProblems.SolveSat;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.AddServiceDefaults();

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen( c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Raijin Sat Solver Service API", Version = "v1" });
});

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddLogging();

// Configure DbContext for SAT problems (using in-memory database for now)
builder.Services.AddDbContext<SatProblemsDbContext>(options =>
    options.UseInMemoryDatabase("SatProblems")
);

// Register background task processing
builder.Services.AddSingleton<IBackgroundSatSolverTaskQueue>(new BackgroundSatSolverTaskQueue(maxQueueSize: 1000));
builder.Services.AddHostedService<SatSolverBackgroundService>();

builder.Services.AddScoped<ISatSolver, Cryptominisat>();
builder.Services.AddOptions<CryptominisatOptions>()
    .Configure((CryptominisatOptions options, IConfiguration configuration) =>
    {
        options.ContainerName = configuration[EnvironmentVariables.Cryptominisat.ContainerName]!;
        options.FileExchangeContainerPath =
            configuration[EnvironmentVariables.Cryptominisat.MountContainerPath]!;
        options.FileExchangeLocalPath =
            configuration[EnvironmentVariables.Cryptominisat.MountLocalPath]!;
    })
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.ContainerName),
        "Cryptominisat container name is required."
    ).Validate(
        options => !string.IsNullOrWhiteSpace(options.FileExchangeContainerPath),
        "Cryptominisat container file exchange path is required."
    ).Validate(
        options => !string.IsNullOrWhiteSpace(options.FileExchangeLocalPath),
        "Cryptominisat local file exchange path is required."
    );

builder.Services.AddScoped<SolveSatCommandHandler>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(
        c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Raijin Sat Solver Service API v1");
            c.RoutePrefix = string.Empty;
            c.DocumentTitle = "Raijin Sat Solver Service API Documentation";
        }
    );
}

app.UseHttpsRedirection();

app.MapDefaultEndpoints();
app.MapEndpoints();

app.Run();

//https://copilot.microsoft.com/shares/n3Yk2v2cy2cDdmot4fupm
// https://copilot.microsoft.com/shares/pages/koPwdVxGsP8cb8YmrB31j