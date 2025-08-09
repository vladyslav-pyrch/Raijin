using Raijin.ProblemSolvingService.Api.Endpoints.V1.CommonSat;
using Raijin.ProblemSolvingService.Application;
using Raijin.ProblemSolvingService.Application.Cqrs;
using Raijin.ProblemSolvingService.Application.Features.CommonSat;
using Raijin.ProblemSolvingService.Application.Features.CommonSat.Commands.SolveSatProblem;
using Raijin.ProblemSolvingService.Application.Features.CommonSat.Commands.SolveSatProblemInternal;
using Raijin.ProblemSolvingService.Domain.SatProblems;
using Raijin.ProblemSolvingService.Infrastructure;
using Raijin.ProblemSolvingService.Infrastructure.Cqrs;
using Raijin.ProblemSolvingService.Infrastructure.CryptoMiniSat;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.AddServiceDefaults();
builder.Services.AddTransient<ICommandHandler<SolveSatProblemCommand, SolveSatProblemCommandResult>,
        SolveSatProblemCommandHandler>();
builder.Services.AddTransient<ICommandHandler<SolveSatProblemInternalCommand, SatResult>,
    SolveSatProblemInternalCommandHandler>();
builder.Services.AddScoped<ICommandDispatcher, CommandQueryDispatcher>();
builder.Services.AddScoped<IQueryDispatcher, CommandQueryDispatcher>();
builder.Services.AddScoped<ISatSolver, CryptominisatSatSolver>();
builder.Services.AddScoped<Cryptominisat>();
builder.Services.AddOptions<CryptominisatOptions>()
    .Configure((CryptominisatOptions options, IConfiguration configuration) =>
    {
        options.ContainerName =
            configuration["CRYPTOMINISAT_CONTAINER_NAME"] ?? throw new InvalidOperationException();
        options.RunCommand =
            configuration["CRYPTOMINISAT_RUN_COMMAND"] ?? throw new InvalidOperationException();
        options.FileExchangeDirectory =
            configuration["CRYPTOMINISAT_FILE_EXCHANGE"] ?? throw new InvalidOperationException();
        options.WorkingDirectory =
            configuration["CRYPTOMINISAT_WORKING_DIRECTORY"] ?? throw new InvalidOperationException();
        options.TimeoutSeconds = 20;
    })
    .Validate(options => options.ContainerName is not null)
    .Validate(options => options.FileExchangeDirectory is not null)
    .Validate(options => options.RunCommand is not null)
    .Validate(options => options.WorkingDirectory is not null)
    .Validate(options => options.TimeoutSeconds >= 0);

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
