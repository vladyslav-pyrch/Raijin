using Raijin.SatSolver.Api.Extensions;
using Raijin.SatSolver.Application;
using Raijin.SatSolver.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.AddServiceDefaults();

// Error handling
builder.Services.AddProblemDetails();

// Security
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// OpenAPI (optional but useful for internal tooling)
builder.Services.AddOpenApi();
builder.Services.AddEndpoints();

// Modules registration
builder.Services.AddInfrastructureApi();
builder.Services.AddApplication();

WebApplication app = builder.Build();

// Error pipeline
app.UseExceptionHandler();

// Zero-trust verification
app.UseAuthentication();
app.UseAuthorization();

// Service endpoints
app.MapDefaultEndpoints();
app.MapEndpoints();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

await app.ApplyMigrations();

app.Run();