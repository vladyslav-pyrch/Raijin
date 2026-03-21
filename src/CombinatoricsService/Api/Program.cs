using Raijin.CombinatoricsService.Api.Extensions;
using Raijin.CombinatoricsService.Api.Middleware;
using Raijin.CombinatoricsService.Application;
using Raijin.CombinatoricsService.Infrastructure;

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

// Correlation context middleware
builder.Services.AddTransient<CorrelationContextMiddleware>();

// Modules registration
builder.Services.AddInfrastructure();
builder.Services.AddApplication();

WebApplication app = builder.Build();

// Error pipeline
app.UseExceptionHandler();

// Correlation context
app.UseMiddleware<CorrelationContextMiddleware>();

// Zero-trust verification
app.UseAuthentication();
app.UseAuthorization();

// Service endpoints
app.MapDefaultEndpoints();
app.MapEndpoints();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.Run();