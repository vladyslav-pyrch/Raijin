using Raijin.Constants;
using Raijin.ProblemSolvingService.Api.Endpoints.V1.CommonSat;
using Raijin.ProblemSolvingService.Application.Cqrs;
using Raijin.ProblemSolvingService.Application.Features.CommonSat;
using Raijin.ProblemSolvingService.Application.Features.CommonSat.Commands.SolveSatProblem;
using Raijin.ProblemSolvingService.Application.Features.CommonSat.Commands.SolveSatProblemInternal;
using Raijin.ProblemSolvingService.Domain.SatProblems;
using Raijin.ProblemSolvingService.Infrastructure.Cqrs;
using Raijin.ProblemSolvingService.Infrastructure.CryptoMiniSat;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Configuration.AddEnvironmentVariables();
builder.Services.AddOpenApi();
builder.AddServiceDefaults();
builder.Services.AddTransient<IRequestHandler<SolveSatProblemCommand, SolveSatProblemCommandResult>,
        SolveSatProblemCommandHandler>();
builder.Services.AddTransient<IRequestHandler<SolveSatProblemInternalCommand, SatResult>,
    SolveSatProblemInternalCommandHandler>();
builder.Services.AddScoped<ISender, DotNetDiSender>();
builder.Services.AddScoped<ISatSolver, CryptominisatSatSolver>();
builder.Services.AddScoped<Cryptominisat>();
builder.Services.AddLogging();
builder.Services.AddOptions<CryptominisatOptions>()
    .Configure((CryptominisatOptions options, IConfiguration configuration) =>
    {
        options.ContainerName = configuration[EnvironmentVariables.Cryptominisat.ContainerName]!;
        options.FileExchangeContainerPath = configuration[EnvironmentVariables.Cryptominisat.FileExchangeContainerPath]!;
        options.FileExchangeLocalPath = configuration[EnvironmentVariables.Cryptominisat.FileExchangeLocalPath]!;
        options.TimeoutSeconds = Convert.ToInt32(configuration[EnvironmentVariables.Cryptominisat.TimeoutSeconds]);

    })
    .Validate(options => !string.IsNullOrWhiteSpace(options.ContainerName), "Cryptominisat container name is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.FileExchangeContainerPath), "Cryptominisat container file exchange path is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.FileExchangeLocalPath), "Cryptominisat local file exchange path is required.")
    .Validate(options => options.TimeoutSeconds >= 0, "Cryptominisat timeout may not be negative.");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapDefaultEndpoints();
app.MapCommonSatEndpoints();

app.Run();
