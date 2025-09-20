using Microsoft.OpenApi.Models;
using Raijin.ProblemSolvingService.Api.Endpoints.V1;
using Raijin.ProblemSolvingService.Application;
using Raijin.ProblemSolvingService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.AddServiceDefaults();

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen( c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Raijin Problem Solving Service API", Version = "v1" });
});

builder.Services.AddLogging();

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(
        c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Raijin Problem Solving Service API v1");
            c.RoutePrefix = string.Empty;
            c.DocumentTitle = "Raijin Problem Solving Service API Documentation";
        }
    );
}

app.UseHttpsRedirection();

app.MapDefaultEndpoints();
app.MapEndpoints();

app.Run();
